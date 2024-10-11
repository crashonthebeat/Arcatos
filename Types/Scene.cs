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

namespace Arcatos.Types
{
    public struct SceneDto
    {
        [JsonInclude] public required string id;
        [JsonInclude] public string name;
        [JsonInclude] public required string summary;
        [JsonInclude] public required string[] desc;
        [JsonInclude] public required int[] coords;
        [JsonInclude] public bool visited;
    }
    
    // Scene is the type for each location in the game, it is the base for Overworld cells and map cells.
    public class Scene : Entity
    {
        public int x { get; set; }      // X Coordinate on Map
        public int y { get; set; }      // Y Coordinate on Map
        public Dictionary<string, Door> Exits { get; set; }
        public override Box Inventory { get; set; }
        // Todo: Add Map Parameter to object.

        public Scene(string id, string name, string summary, string[] desc, int[] coords, bool isKnown = false) 
                   : base(id, summary, desc, name, isKnown)
        {
            this.EntityType = "scene";
            this.x = coords[0];
            this.y = coords[1];
            this.Exits = new Dictionary<string, Door>();
            this.Inventory = new Box(this, BoxType.Int);
        }

        public Scene(SceneDto dto) : this(dto.id, dto.name, dto.summary, dto.desc, dto.coords, dto.visited)
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
                    Console.WriteLine("You see the following exits:");
                    break;
                case int x when x == 1:
                    Console.WriteLine("You see one exit:");
                    break;
                default:
                    Console.WriteLine("You do not see any way out.");
                    break;
            }

            Console.ResetColor();

            // Enumerate and Display Exits
            foreach (KeyValuePair<string, Door> exit in this.Exits)
            {
                // Write Direction in Light Blue with no new line
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write($"{exit.Key.ToUpper()}: ");
                Console.ResetColor();

                // If next scene has been visited, display the name, otherwise display its short description.
                Scene newRoom = exit.Value.Adjacencies[this];
                if (newRoom.IsKnown && CheckExitDistance(newRoom) && (newRoom.Name != null))
                {
                    Game.Narrate([newRoom.Name]);
                }
                else
                {
                    Game.Narrate([newRoom.Glance()]);
                }
            }
        }

        // On Map Construction, this is the per-scene method that processes all the exits into Scenes.
        public void AddExit(Door exit)
        {            
            // Get Other Room
            Scene scene = exit.Adjacencies[this];

            // Get Direction of other room
            Dir dir = Calc.Direction(this, scene);
            string direction = dir.ToString();

            Dev.Log($"Adding{scene.Name} is {direction} of {this.Name} via {exit.summary}");

            this.Exits.Add(direction, exit);
        }

        // Checks if a scene is close enough to see past an exit
        // Scenes should only have a distance greater than 1 when connecting them via a hallway, cavern, or other space
        // where a separate room does not make sense. 
        private bool CheckExitDistance(Scene scene)
        {
            double xdist = this.x - scene.x;
            double ydist = this.y - scene.y;
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
