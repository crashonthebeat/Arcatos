using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Arcatos.Utils;
using Arcatos.Types.Interfaces;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

namespace Arcatos.Types
{
    public struct SceneDto
    {
        [JsonInclude] public required string id;
        [JsonInclude] public string name;
        [JsonInclude] public required string summary;
        [JsonInclude] public required string[] desc;
        [JsonInclude] public required int[] nw_corner;
        [JsonInclude] public required int[] se_corner;
        [JsonInclude] public bool visited;
    }
    
    // Scene is the type for each location in the game, it is the base for Overworld cells and map cells.
    public class Scene : Entity
    {
        public (int x, int y) CornerNW;     // Coordinates (x,y) northwestern cell center.
        public (int x, int y) CornerSE;     // Coordinates of southeastern cell center.
        public Dictionary<string, Exit> Exits { get; set; }
        public override Box Inventory { get; set; }
        private (double n, double e, double s, double w) wallPos;
        private (double x, double y) center;
        // Todo: Add Map Parameter to object.

        public Scene(string id, string name, string summary, string[] desc, int[] nwcorner, int[] secorner, bool isKnown = false) 
                   : base(id, summary, desc, name, isKnown)
        {
            this.EntityType = "scene";
            this.CornerNW        = (nwcorner[0], nwcorner[1]);
            this.CornerSE       = (secorner[0], secorner[1]);
            this.Exits      = new Dictionary<string, Exit>();
            this.Inventory  = new Box(this, BoxType.Int);
            this.wallPos    = GetWallPositions();
            this.center     = this.GetRoomCenter();
        }

        public Scene(SceneDto dto) : this(dto.id, dto.name, dto.summary, dto.desc, dto.nw_corner, dto.se_corner, dto.visited)
        {
            // Scene Constructor for initialization with json file.
        }

        // Enter is the narration that is displayed when the player enters a room.
        public void Enter()
        {            
            // Print Scene Title
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"==( {this.Name.ToUpper()} )==");
            Console.ResetColor();

            // Print Description and Exits
            this.Examine();
            this.ListExits();
        }

        public void ListExits()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            // Exits Header
            switch (this.Exits.Count) {
                case int x when x > 1:
                    Game.Narrate("You see the following exits:");
                    break;
                case int x when x == 1:
                    Game.Narrate("You see one exit:");
                    break;
                default:
                    Game.Narrate("You do not see any way out.");
                    break;
            }

            Console.ResetColor();

            // Enumerate and Display Exits
            foreach (KeyValuePair<string, Exit> exit in this.Exits)
            {
                // Write Direction in Light Blue with no new line
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write($"{exit.Key.ToUpper()}: ");
                Console.ResetColor();

                // If the exit is closed, display the glance of the exit.
                // If next scene has been visited, display the name, otherwise display its short description.
                Scene newRoom = exit.Value.Adjacencies[this];
                if (exit.Value.IsClosed || exit.Value.IsLocked)
                {
                    Game.Narrate(exit.Value.Glance());
                }
                else if (newRoom.IsKnown && (newRoom.Name != null))
                {
                    Game.Narrate(newRoom.Name);
                }
                else
                {
                    Game.Narrate(newRoom.Glance());
                }
            }
        }

        // On Map Construction, this is the per-scene method that processes all the exits into Scenes.
        public void AddExit(Exit exit)
        {            
            // Get Other Room
            Scene scene = exit.Adjacencies[this];

            // Get Direction of other room
            Dir dir = Calc.Direction(this, scene);
            string direction = dir.ToString();

            Dev.Log($"Adding{scene.Name} is {direction} of {this.Name} via {exit.summary}");

            this.Exits.Add(direction, exit);
        }

        public bool AddExit(Dir dir, Exit exit) {
            // Make it into a string because I'm dumb and made two different things to represent direction.
            string? dirString = Enum.GetName(dir.GetType(), dir);
            if (!String.IsNullOrEmpty(dirString) && !this.Exits.Keys.Contains(dirString))
            {
                // Add this exit and direction
                Dev.Log($"{exit.id} added at {dirString}");
                this.Exits.Add(dirString, exit);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool GetExitDirection(Exit exit, Dir wall, int conf)
        {
            double xCornerDist = Calc.SceneCornerWidth(this.CornerSE.x - this.CornerNW.x);
            double yCornerDist = Calc.SceneCornerWidth(this.CornerSE.y - this.CornerNW.y);
            double xMaxDist, yMaxDist;


            // Set the max and min corner distances
            if (xCornerDist > yCornerDist)
            {
                // Set the corner distance to the lowest value, and set the max value of the longest side
                xMaxDist = xCornerDist;
                xCornerDist = yCornerDist;
                // y max distance does not change from min distance
                yMaxDist = yCornerDist;
            }
            else
            {
                // And vice Versa
                yMaxDist = yCornerDist;
                yCornerDist = xCornerDist;
                xMaxDist = xCornerDist;
            }

            // Let's first get the margins for each confidence level.
            (double xDist, double yDist) = conf switch
            {
                2 => (xCornerDist, yCornerDist), // Least tolerance for the corners
                1 => (xMaxDist, yMaxDist),       // Most tolerance for corners
                0 => (this.center.x - this.wallPos.w, this.center.y - this.wallPos.n),
                _ => (this.center.x - this.wallPos.w, this.center.y - this.wallPos.n)
            };

            // You know instead of conf levels and fancy formulas why don't i just a simple addition thing. 
            // For some reason, it's taking cells that should be pretty southward and making them center with higher conf.

            Dev.Log($"* * E corner is < {this.wallPos.e + xDist} and W corner is > {this.wallPos.w - xDist}");
            Dev.Log($"* * N Corner is < {this.wallPos.n + yDist} and S corner is > {this.wallPos.s - yDist}");
            Dev.Log($"* * N/S Center is between {this.center.x - (xDist / 2)} and {this.center.x + (xDist / 2)}");
            Dev.Log($"* * E/W Center is between {this.center.y - (yDist / 2)} and {this.center.y + (yDist / 2)}");

            // Below this line I reused the logic from GetExitLocation that i used earlier today, but with the new and improved formulas.
            // Lets see if it works

            // Confident exit is in northwest corner
            if      ((wall == Dir.north && exit.Loc.x > this.wallPos.w - xDist && exit.Loc.x < this.wallPos.e )     // west corner of north wall
                  || (wall == Dir.west  && exit.Loc.y > this.wallPos.n && this.wallPos.n + yDist > exit.Loc.y))    // north corner of west wall
            {
                Dev.Log("nw conf");
                if (!this.Exits.ContainsKey("northwest"))
                {
                    return this.AddExit(Dir.northwest, exit);
                }
                else if (!this.Exits.ContainsKey("west") && conf <= 1 && wall == Dir.west)
                {
                    return this.AddExit(Dir.west, exit);
                }
                else if (!this.Exits.ContainsKey("north") && conf <= 1 && wall == Dir.north)
                {
                    return this.AddExit(Dir.north, exit);
                }
            }
            // Confident exit is in northeast corner
            else if ((wall == Dir.north && exit.Loc.x < this.wallPos.e && exit.Loc.x > this.wallPos.e - xDist)     // east corner of north wall
                  || (wall == Dir.east  && exit.Loc.y > this.wallPos.n && this.wallPos.n + yDist > exit.Loc.y))    // north corner of east wall
            {
                Dev.Log("ne conf");
                if (!this.Exits.ContainsKey("northeast"))
                {
                    return this.AddExit(Dir.northeast, exit);
                }
                else if (!this.Exits.ContainsKey("east") && conf <= 1 && wall == Dir.east)
                {
                    return this.AddExit(Dir.east, exit);
                }
                else if (!this.Exits.ContainsKey("north") && conf <= 1 && wall == Dir.north)
                {
                    return this.AddExit(Dir.north, exit);
                }
            }
            // Confident exit is in southwest corner
            else if ((wall == Dir.south && exit.Loc.x > this.wallPos.w - xDist && exit.Loc.x < this.wallPos.e)     // west corner of south wall
                  || (wall == Dir.west  && exit.Loc.y > this.wallPos.s - yDist && exit.Loc.y < this.wallPos.s))    // south corner of west wall
            {
                Dev.Log("sw conf");
                if (!this.Exits.ContainsKey("southwest"))
                {
                    return this.AddExit(Dir.southwest, exit);
                }
                else if (!this.Exits.ContainsKey("west") && conf <= 1 && wall == Dir.west)
                {
                    return this.AddExit(Dir.west, exit);
                }
                else if (!this.Exits.ContainsKey("south") && conf <= 1 && wall == Dir.south)
                {
                    return this.AddExit(Dir.south, exit);
                }
            }
            // Confident exit is in southeast corner
            else if ((wall == Dir.south && exit.Loc.x < this.wallPos.e && exit.Loc.x > this.wallPos.e - xDist)     // east corner of south wall
                  || (wall == Dir.east  && exit.Loc.y > this.wallPos.s - yDist && exit.Loc.y < this.wallPos.s ))    // south corner of east wall
            {
                Dev.Log("se conf");
                if (!this.Exits.ContainsKey("southeast"))
                {
                    return this.AddExit(Dir.southeast, exit);
                }
                else if (!this.Exits.ContainsKey("east") && conf <= 1 && wall == Dir.east)
                {
                    return this.AddExit(Dir.east, exit);
                }
                else if (!this.Exits.ContainsKey("south") && conf <= 1 && wall == Dir.south)
                {
                    return this.AddExit(Dir.south, exit);
                }
            }
            // Confident exit is center north
            if (wall == Dir.north && !this.Exits.ContainsKey("north") 
                  && this.center.x - (xDist / 2) < exit.Loc.x && exit.Loc.x < this.center.x + (xDist / 2))
            {
                Dev.Log("n center conf");
                return this.AddExit(Dir.north, exit);
            }
            // Confident exit is center east
            else if (wall == Dir.east && !this.Exits.ContainsKey("east")
                  && this.center.y - (yDist / 2) < exit.Loc.y && exit.Loc.x < this.center.y + (yDist / 2))
            {
                Dev.Log("e center conf");
                return this.AddExit(Dir.east, exit);
            }
            // Confident exit is center south
            else if (wall == Dir.south && !this.Exits.ContainsKey("south")
                  && this.center.x - (xDist / 2) < exit.Loc.x && exit.Loc.x < this.center.x + (xDist / 2))
            {
                Dev.Log("s center conf");
                return this.AddExit(Dir.south, exit);
            }
            // Confident exit is center west
            else if (wall == Dir.west && !this.Exits.ContainsKey("west")
                  && this.center.y - (yDist / 2) < exit.Loc.y && exit.Loc.x < this.center.y + (yDist / 2))
            {
                Dev.Log("w center conf");
                return this.AddExit(Dir.west, exit);
            }
            else
            {
                return false;
            }
        }

        // This will loop through a list of exits to add to the scene, and continue until the list is empty
        // In other words, loop until all exits have been added to the scene. 
        // It does this by assigning a confidence level needed to put the exit in a certain direction.
        // For the first go-around, the function will look for anything dead-center on the directions.
        // Examples being scene center on a wall, or exactly on a corner.
        // Next, it's going to look for anything within a specific corner distance to place in a corner
        // Outside of that distance would be a wall direction. 
        // The margin gets larger as the confidence decreases until basically anything is 
        public void AddExits(List<Exit> exits)
        {
            int i = 0;
            int conf = 3;
            (double x, double y) cent = GetRoomCenter();
            Dev.Log($"* Walls at N: {this.wallPos.n}, E: {this.wallPos.e}, S: {this.wallPos.s}, W: {this.wallPos.w}");
            Dev.Log($"* Center at {cent.x}, {cent.y}");

            while (exits.Count > 0)
            {
                Exit exit = exits[i];
                Dev.Log($"*i{i} {exit.id} at {exit.Loc.x}, {exit.Loc.y}");

                switch ((exit.Loc.x, exit.Loc.y))
                {
                    // Exit is at northeast corner
                    case (double x, double y) when x == this.wallPos.e && y == this.wallPos.n: 
                        this.AddExit(Dir.northeast, exit);
                        break;

                    // Exit is at northwest corner
                    case (double x, double y) when x == this.wallPos.w && y == this.wallPos.n:
                        this.AddExit(Dir.northwest, exit);
                        break;

                    // Exit is at southeast corner
                    case (double x, double y) when x == this.wallPos.e && y == this.wallPos.s:
                        this.AddExit(Dir.southeast, exit);
                        break;

                    // Exit is at southwest corner
                    case (double x, double y) when x == this.wallPos.w && y == this.wallPos.s:
                        this.AddExit(Dir.southwest, exit);
                        break;

                    // Exit is dead center north
                    case (double x, double y) when x == cent.x && y == this.wallPos.n:
                        this.AddExit(Dir.north, exit);
                        break;

                    // Exit is dead center south
                    case (var x, var y) when x == cent.x && y == this.wallPos.s:
                        this.AddExit(Dir.south, exit);
                        break;

                    // Exit is dead center east
                    case (var x, var y) when x == this.wallPos.e && y == cent.y:
                        this.AddExit(Dir.east, exit);
                        break;

                    // Exit is dead center west
                    case (var x, var y) when x == this.wallPos.w && y == cent.y:
                        this.AddExit(Dir.west, exit);
                        break;

                    // Exit is on east wall
                    case (var x, var _) when x == this.wallPos.e && conf < 3:
                        Dev.Log($"* Checking east wall for {exit.id}");
                        this.GetExitDirection(exit, Dir.east, conf);
                        break;
                        
                    // Exit is on west wall
                    case (var x, var _) when x == this.wallPos.w && conf < 3:
                        Dev.Log($"* Checking west wall for {exit.id}");
                        this.GetExitDirection(exit, Dir.west, conf);
                        break;

                    // Exit is on north wall
                    case (var _, var y) when y == this.wallPos.n && conf < 3:
                        Dev.Log($"* Checking north wall for {exit.id}");
                        this.GetExitDirection(exit, Dir.north, conf);
                        break;

                    // Exit is on south wall
                    case (var _x, var y) when y == this.wallPos.s && conf < 3:
                        Dev.Log($"* Checking south wall for {exit.id}");
                        this.GetExitDirection(exit, Dir.south, conf);
                        break;

                    default:
                        break;
                }

                // Increment but keep looping through list.
                i++;
                if (i >= exits.Count)
                {
                    // Once we reached the end of the loop, go back to 0.
                    i = 0;
                    conf--;
                    Dev.Log($"LOOP ENDED: CONFIDENCE TO {conf}");
                    exits = exits.Except(this.Exits.Values).ToList();
                }
                Dev.Log($"Remaining to add: {exits.Count}");
                if (conf < -1 && exits.Count > 0)
                {
                    this.ListExits();
                    throw new WeastException($"Cannot place all exits for {this.id}.");
                }
            }
        }

        // Load Room Inventory (from world load)
        public void LoadItems(Dictionary<string, int> itemDefs)
        {
            foreach (string itemid in itemDefs.Keys)
            {
                Dev.Log(itemDefs[itemid].ToString());
                this.Inventory.Items.Add(Game.Items[itemid], itemDefs[itemid]);
            }
        }

        public (double, double, double, double) GetWallPositions()
        {
            double n = this.CornerNW.y - 0.5;
            double w = this.CornerNW.x - 0.5;
            double s = this.CornerSE.y + 0.5;
            double e = this.CornerSE.x + 0.5;

            return (n, e, s, w);
        }

        public (double, double) GetRoomCenter()
        {
            double x = (this.wallPos.e + this.wallPos.w) / 2;
            double y = (this.wallPos.n + this.wallPos.s) / 2;

            return (x, y);
        }
    }
}
