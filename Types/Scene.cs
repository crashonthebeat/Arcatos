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

namespace Arcatos.Types
{
    public struct SceneDto
    {
        [JsonInclude] public required string id;
        [JsonInclude] public string name;
        [JsonInclude] public required string summary;
        [JsonInclude] public required string[] desc;
        [JsonInclude] public required int[] coords;
        [JsonInclude] public required int[] size;
        [JsonInclude] public bool visited;
    }
    
    // Scene is the type for each location in the game, it is the base for Overworld cells and map cells.
    public class Scene : Entity
    {
        public (int x, int y) CornerNW;     // Coordinates (x,y) northwestern cell center.
        public (int x, int y) CornerSE;     // Coordinates of southeastern cell center.
        public Dictionary<string, Exit> Exits { get; set; }
        public override Box Inventory { get; set; }
        public Dictionary<Exit, List<Dir>> ExitPairs;  // Holding property for possible exits.
        private (double n, double e, double s, double w) wallPos;
        // Todo: Add Map Parameter to object.

        public Scene(string id, string name, string summary, string[] desc, int[] coords, int[] size, bool isKnown = false) 
                   : base(id, summary, desc, name, isKnown)
        {
            this.EntityType = "scene";
            this.CornerNW        = (coords[0], coords[1]);
            this.CornerSE       = (this.CornerNW.x + size[0], this.CornerNW.y + size[1]);
            this.Exits      = new Dictionary<string, Exit>();
            this.Inventory  = new Box(this, BoxType.Int);
            this.ExitPairs  = new Dictionary<Exit, List<Dir>>();
            this.wallPos    = GetWallPositions();
        }

        public Scene(SceneDto dto) : this(dto.id, dto.name, dto.summary, dto.desc, dto.coords, dto.size, dto.visited)
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

        // I'm so sorry for what I am about to do. This is either genius or insanity. 
        // This kind of works like arp, except spanning tree hasn't been invented.
        // TODO: Invent spanning tree before I even think about running this code.
        public bool ResolveExits()
        {
            // This first loop checks for easily resolvable exits.
            foreach (KeyValuePair<Exit, List<Dir>> kvp in this.ExitPairs)
            {
                Exit exit = kvp.Key;
                // Only one possible direction for this exit.
                if (kvp.Value.Count == 1)
                {
                    Scene opp = exit.Adjacencies[this];
                    Dir oppDir = Calc.OppDir(kvp.Value[0]);

                    bool resolved = opp.ResolveAdjacent(oppDir, exit);

                    // If the opposite exit could be resolved, then resolve the one in this scene as well.
                    if (resolved)
                    {
                        this.AddExit(kvp.Value[0], exit);
                        this.ExitPairs.Remove(exit);
                        continue;
                    }
                    // I'm a baby coder and even I know that this line is going to give me a BAD time.
                    // For my networking friends, this is what we call a BROADCAST STORM.
                    else if (opp.ResolveExits())
                    {
                        // Opposite scene has been resolved
                        this.AddExit(kvp.Value[0], exit);
                        this.ExitPairs.Remove(exit);
                        continue;
                    }
                }
                else
                {
                    foreach (Dir dir in kvp.Value)
                    {
                        bool resolved = this.ResolveAdjacent(dir, exit);
                        // TODO: finish this
                    }
                }
            }

            return this.ExitPairs.Count == 0;
        }

        public bool ResolveAdjacent(Dir dir, Exit exit)
        {
            // Check to see if the given direction is a possible direction for any other exit.
            foreach (KeyValuePair<Exit, List<Dir>> kvp in this.ExitPairs)
            {
                if (kvp.Value.Contains(dir) && kvp.Key != exit)
                {
                    // If so, the adjacency cannot yet be resolved.
                    return false;
                }
            }

            // At this point there is no other exit that could use this direction so it will be claimed.
            this.AddExit(dir, exit);
            this.ExitPairs.Remove(exit);
            return true;
        }

        public bool AddExit(Dir dir, Exit exit) {
            // Make it into a string because I'm dumb and made two different things to represent direction.
            string? dirString = Enum.GetName(dir.GetType(), dir);
            if (!String.IsNullOrEmpty(dirString) && !this.Exits.Keys.Contains(dirString))
            {
                // Add this exit and direction
                this.Exits.Add(dirString, exit);
                return true;
            }
            else
            {
                return false;
            }
        }

        // This method will resolve an exit direction based on an integer that states the level of accuracy
        public bool GetExitDirection(Exit exit, Dir wall, int conf)
        {
            double xCornerDist = Calc.SceneCornerWidth(this.CornerSE.x - this.CornerNW.x);
            double yCornerDist = Calc.SceneCornerWidth(this.CornerSE.y - this.CornerNW.x);
            double minDist = Math.Max(xCornerDist, yCornerDist);

            double check = conf switch
            {
                90 => Math.Min(xCornerDist, yCornerDist),
                75 => Math.Max(xCornerDist, yCornerDist),
                _ => 0
            };

            bool added;
            // Confident exit is in northwest corner
            if      ((wall == Dir.north && exit.Loc.x <= this.CornerNW.x + minDist)
                  || (wall == Dir.west  && exit.Loc.y <= this.CornerNW.y + minDist))
            {
                added = this.AddExit(Dir.northwest, exit);
            }
            // Confident exit is in northeast corner
            else if ((wall == Dir.north && exit.Loc.x >= this.CornerSE.x - minDist)
                  || (wall == Dir.east  && exit.Loc.y <= this.CornerNW.y + minDist))
            {
                added = this.AddExit(Dir.northeast, exit);
            }
            // Confident exit is in southwest corner
            else if ((wall == Dir.south && exit.Loc.x <= this.CornerNW.x + minDist)
                  || (wall == Dir.west  && exit.Loc.y >= this.CornerSE.y - minDist))
            {
                added = this.AddExit(Dir.southwest, exit);
            }
            // Confident exit is in southeast corner
            else if ((wall == Dir.south && exit.Loc.x >= this.CornerSE.x - minDist)
                  || (wall == Dir.east  && exit.Loc.y >= this.CornerSE.y - minDist))
            {
                added = this.AddExit (Dir.southeast, exit);
            }
        }

        public void AddExits(List<Exit> exits)
        {
            int i = 0;
            //double xCornerDist = Calc.SceneCornerWidth(this.CornerSE.x - this.CornerNW.x);
            //double yCornerDist = Calc.SceneCornerWidth(this.CornerSE.y - this.CornerNW.x);
            (double x, double y) cent = GetRoomCenter();

            while (exits.Count > 0)
            {
                Exit exit = exits[i];

                Dir wall;
                bool corner;
                switch (exit.Loc)
                {
                    // Exit is at northeast corner
                    case (double x, double y) when (x == this.wallPos.e && y == this.wallPos.n): 
                        this.AddExit(Dir.northeast, exit);
                        exits.Remove(exit);
                        continue;

                    // Exit is at northwest corner
                    case (double x, double y) when (x == this.wallPos.w && y == this.wallPos.n):
                        this.AddExit(Dir.northwest, exit);
                        exits.Remove(exit);
                        continue;

                    // Exit is at southeast corner
                    case (double x, double y) when (x == this.wallPos.e && y == this.wallPos.s):
                        this.AddExit(Dir.southeast, exit);
                        exits.Remove(exit);
                        continue;

                    // Exit is at southwest corner
                    case (double x, double y) when (x == this.wallPos.w && y == this.wallPos.s):
                        this.AddExit(Dir.southwest, exit);
                        exits.Remove(exit);
                        continue;

                    // Exit is dead center north
                    case (double x, double y) when (x == cent.x && y == this.wallPos.n):
                        this.AddExit(Dir.north, exit);
                        exits.Remove(exit);
                        continue;

                    // Exit is dead center south
                    case (var x, var y) when (x == cent.x && y == this.wallPos.s):
                        this.AddExit(Dir.north, exit);
                        exits.Remove(exit);
                        continue;

                    // Exit is dead center east
                    case (var x, var y) when (x == this.wallPos.e && y == cent.y):
                        this.AddExit(Dir.east, exit);
                        exits.Remove(exit);
                        continue;

                    // Exit is dead center west
                    case (var x, var y) when (x == this.wallPos.w && y == cent.y):
                        this.AddExit(Dir.west, exit);
                        exits.Remove(exit);
                        continue;

                    // Exit is on east wall
                        case (var x, var _) when x == this.wallPos.e:
                        wall = Dir.east;
                        break;

                    // Exit is on west wall
                    case (var x, var _) when x == this.wallPos.w:
                        wall = Dir.west;
                        break;

                    // Exit is on north wall
                    case (var _, var y) when y == this.wallPos.n:
                        wall = Dir.north;
                        break;

                    // Exit is on south wall
                    case (var _x, var y) when y == this.wallPos.s:
                        wall = Dir.south;
                        break;

                    default:
                        throw new WeastException();
                }



                // Increment but keep looping through list.
                i = (i + 1) % exits.Count;
            }
        }

        // Checks if a scene is close enough to see past an exit
        // Scenes should only have a distance greater than 1 when connecting them via a hallway, cavern, or other space
        // where a separate room does not make sense. 
        private bool CheckExitDistance(Scene scene)
        {
            double xdist = this.CornerNW.x - scene.CornerNW.x;
            double ydist = this.CornerNW.y - scene.CornerNW.y;
            double ddist = Math.Pow(Math.Pow(xdist, 2) + Math.Pow(ydist, 2), 0.5);

            return ((xdist < 3) && (ydist < 3) && (ddist < 3));
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
            double x = this.CornerNW.x + (this.CornerSE.x / 2);
            double y = this.CornerNW.y + (this.CornerSE.y / 2);

            return (x, y);
        }
    }
}
