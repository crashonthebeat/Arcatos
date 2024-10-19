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
        public (int x, int y) loc;      // Coordinates (x,y) of top left corner of room.
        public (int x, int y) size;     // Length and Width of Scene
        public Dictionary<string, Exit> Exits { get; set; }
        public override Box Inventory { get; set; }
        public Dictionary<Exit, List<Dir>> ExitPairs;  // Holding property for possible exits.
        // Todo: Add Map Parameter to object.

        public Scene(string id, string name, string summary, string[] desc, int[] coords, int[] size, bool isKnown = false) 
                   : base(id, summary, desc, name, isKnown)
        {
            this.EntityType = "scene";
            this.loc        = (coords[0], coords[1]);
            this.size       = (size[0], size[1]);
            this.Exits      = new Dictionary<string, Exit>();
            this.Inventory  = new Box(this, BoxType.Int);
            this.ExitPairs  = new Dictionary<Exit, List<Dir>>();
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

                // If next scene has been visited, display the name, otherwise display its short description.
                Scene newRoom = exit.Value.Adjacencies[this];
                if (newRoom.IsKnown && CheckExitDistance(newRoom) && (newRoom.Name != null))
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

        public void AddExit(Dir dir, Exit exit) {
            // Make it into a string because I'm dumb and made two different things to represent direction.
            string? dirString = Enum.GetName(dir.GetType(), dir);
            if (!String.IsNullOrEmpty(dirString))
            {
                // Add this exit and direction
                this.Exits.Add(dirString, exit);
                // Remove this pair from the checks.
                this.ExitPairs.Remove(exit);
                // Tell calling function we're good.
            }
            else
            {
                // string shouldn't be empty but if i don't do this visual studio gets mad at me :(
                throw new Exception(); // TODO: Invent new exception
            }
        }

        // Checks if a scene is close enough to see past an exit
        // Scenes should only have a distance greater than 1 when connecting them via a hallway, cavern, or other space
        // where a separate room does not make sense. 
        private bool CheckExitDistance(Scene scene)
        {
            double xdist = this.loc.x - scene.loc.x;
            double ydist = this.loc.y - scene.loc.y;
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
    }
}
