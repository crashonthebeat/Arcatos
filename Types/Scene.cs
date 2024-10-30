using Arcatos.Types.Interfaces;
using Arcatos.Types.Items;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Arcatos.Utils;

namespace Arcatos.Types
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct SceneDto
    {
        [JsonInclude] public required string id;
        [JsonInclude] public string name;
        [JsonInclude] public required string summary;
        [JsonInclude] public required string[] desc;
        [JsonInclude] public bool visited;
    }
    
    // Scene is the type for each location in the game, it is the base for Overworld cells and map cells.
    public sealed class Scene : Entity, IBox
    {
        public (int x, int y)        CornerNW; // Coordinates (x,y) northwestern cell center.
        public (int x, int y)        CornerSE; // Coordinates of southeastern cell center.
        public Dictionary<Dir, Exit> Exits     { get; }
        public Box                   Inventory { get; }
        
        private readonly (double n, double e, double s, double w) _wallPos;
        private readonly (double x, double y)                     _center;
        // Todo: Add Map Parameter to object.

        private new const bool Debug = false;

        private Scene(string id, string name, string summary, string[] desc, int[] nwCorner, int[] seCorner, bool isKnown = false) 
                   : base(id, summary, desc, name, isKnown)
        {
            this.CornerNW   = (nwCorner[0], nwCorner[1]);
            this.CornerSE   = (seCorner[0], seCorner[1]);
            this.Exits      = new Dictionary<Dir, Exit>();
            this.Inventory  = new Box(this, BoxType.Int);
            this._wallPos   = this.GetWallPositions();
            this._center    = this.GetRoomCenter();
        }

        public Scene(SceneDto dto, LayoutDto layout) : this(dto.id, dto.name, dto.summary, dto.desc, layout.CornerNW, layout.CornerSE, dto.visited)
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
            if (this.Inventory.Items.Count > 0) this.ListItems();
            this.ListExits();
            this.Learn();
        }

        public void ListItems()
        {
            this.Inventory.ListItems();
        }

        private void ListExits()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            // Exits Header
            switch (this.Exits.Count) {
                case > 1:
                    Game.Narrate("You see the following exits:");
                    break;
                case 1:
                    Game.Narrate("You see one exit:");
                    break;
                default:
                    Game.Narrate("You do not see any way out.");
                    break;
            }

            Console.ResetColor();

            // Enumerate and Display Exits
            foreach (KeyValuePair<Dir, Exit> exit in this.Exits)
            {
                if (exit.Value.IsHidden) continue;

                // Write Direction in Light Blue with no new line
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write($"{exit.Key.ToString().ToUpper()}: ");
                Console.ResetColor();

                // If the exit is closed, display the glance of the exit.
                // If next scene has been visited, display the name, otherwise display its short description.
                Scene newRoom = exit.Value.AdjScenes[this];
                
                if (exit.Value.IsClosed || exit.Value.IsLocked)
                {
                    Game.Narrate(exit.Value.Glance());
                }
                else if (newRoom.IsKnown)
                {
                    Game.Narrate(newRoom.Name);
                }
                else
                {
                    Game.Narrate(newRoom.Glance());
                }
            }
        }

        // Scene summaries are going to be a bit more dynamic than just "a/an blankety blank blank" so this needed to be overridden.
        public override string Glance()
        {
            return this.IsKnown ? this.Name : $"{this.summary}";
        }

        // On Map Construction, this is the per-scene method that processes all the exits into Scenes.
        // Check both scenes for the exit and if they don't match, see which direction makes more sense.
        // If the two exits are so different that they can't be resolved or they can't be moved, leave it be.
        public void AddExit(Dir dir, Exit exit) {
            this.Exits.Add(dir, exit);
            // Add this exit and direction
            Dev.Log($"{exit.id} added at {dir.ToString()}", Scene.Debug);
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
            // This gets a list of exits with all properties, including coordinates of said exit
            // This function will then determine where the exit gets placed.
            double xCornerDist = Calc.SceneCornerWidth(this.CornerSE.x - this.CornerNW.x);
            double yCornerDist = Calc.SceneCornerWidth(this.CornerSE.y - this.CornerNW.y);
            
            int i    = 0;
            int conf = 0;
            
            // Since some exits will have ambiguous location, the function needs to first place any that are specific.
            // On the first run, the function only looks for those exits on dead center or directly on the corners.
            // On each subsequent run, the tolerance for center and corner is increased by a little bit.
            while (exits.Count > 0)
            {
                Exit exit = exits[i];
                double xTolerance = conf * xCornerDist * 0.75;
                double yTolerance = conf * yCornerDist * 0.75;
                
                switch (exit.Loc.x, exit.Loc.y)
                {
                    // Exit is at northeast corner
                    case var (x, y) when Math.Abs( x - this._wallPos.e ) <= xTolerance 
                                         && Math.Abs( y - this._wallPos.n) <= yTolerance
                                         && !this.Exits.ContainsKey(Dir.northeast): 
                        this.AddExit(Dir.northeast, exit);
                        break;

                    // Exit is at northwest corner
                    case var (x, y) when Math.Abs( x - this._wallPos.w ) <= xTolerance 
                                         && Math.Abs( y - this._wallPos.n) <= yTolerance
                                         && !this.Exits.ContainsKey(Dir.northwest):
                        this.AddExit(Dir.northwest, exit);
                        break;

                    // Exit is at southeast corner
                    case var (x, y) when Math.Abs( x - this._wallPos.e ) <= xTolerance 
                                         && Math.Abs( y - this._wallPos.s) <= yTolerance
                                         && !this.Exits.ContainsKey(Dir.southeast):
                        this.AddExit(Dir.southeast, exit);
                        break;

                    // Exit is at southwest corner
                    case var (x, y) when Math.Abs( x - this._wallPos.w ) <= xTolerance 
                                         && Math.Abs( y - this._wallPos.s) <= yTolerance
                                         && !this.Exits.ContainsKey(Dir.southwest):
                        this.AddExit(Dir.southwest, exit);
                        break;

                    // Exit is center north
                    case var (x, y) when Math.Abs( x - this._center.x ) <= xTolerance 
                                         && Math.Abs( y - this._wallPos.n ) <= 0
                                         && !this.Exits.ContainsKey(Dir.north):
                        this.AddExit(Dir.north, exit);
                        break;

                    // Exit is dead center south
                    case var (x, y) when Math.Abs( x - this._center.x ) <= xTolerance 
                                         && Math.Abs( y - this._wallPos.s ) <= 0
                                         && !this.Exits.ContainsKey(Dir.south):
                        this.AddExit(Dir.south, exit);
                        break;

                    // Exit is dead center east
                    case var (x, y) when Math.Abs( y - this._center.y ) <= yTolerance 
                                         && Math.Abs( x - this._wallPos.e ) <= 0
                                         && !this.Exits.ContainsKey(Dir.east):
                        this.AddExit(Dir.east, exit);
                        break;

                    // Exit is dead center west
                    case var (x, y) when Math.Abs( y - this._center.y ) <= yTolerance 
                                         && Math.Abs( x - this._wallPos.w ) <= 0
                                         && !this.Exits.ContainsKey(Dir.west):
                        this.AddExit(Dir.west, exit);
                        break;
                }

                // Increment but keep looping through list.
                i++;
                if (i >= exits.Count)
                {
                    // Once we reached the end of the loop, go back to 0.
                    i = 0;
                    conf++;
                    exits = exits.Except(this.Exits.Values).ToList();
                }

                // Max out at 10 loops. 
                if (conf == 10)
                {
                    this.ListExits();
                    break;
                }
            }

            if (exits.Count > 0)
            {
                
                Dev.Log($"N: {this._wallPos.n}, E: {this._wallPos.e}, S: {this._wallPos.s}, W: {this._wallPos.w}", Scene.Debug);
                Dev.Log($"Center at {this._center.x}, {this._center.y}", Scene.Debug);
                Dev.Log($"Confidence: X: {conf * xCornerDist * 0.075}, Y: {conf * yCornerDist * 0.075}", Scene.Debug);
                Dev.Log("Remaining Exits:", Scene.Debug);
                foreach (Exit exit in exits)
                {
                    Dev.Log($"{exit.id}: {exit.Loc.x},{exit.Loc.y}", Scene.Debug);
                }

                throw new WeastException($"Could not place all exits in {this.id}");
            }
        }

        // Load Room Inventory (from world load)
        public void LoadItems(Dictionary<string, int> itemDefs)
        {
            foreach (string itemId in itemDefs.Keys)
            {
                Dev.Log(itemId, Scene.Debug);
                Item item = itemId switch
                {
                    not null when Game.Catalog.ContainsKey(itemId)   => Game.Catalog[itemId],
                    not null when Game.Templates.ContainsKey(itemId) => new Item(itemId, Game.Templates[itemId]),
                    _                                                => throw new Exception($"Could not load item {itemId}")
                };
                this.Inventory.Items.Add(item, itemDefs[itemId]);
            }
        }

        // This function isn't needed anymore since I set the coordinates to be the wall corner positions.
        // But I'm lazy and superstitious.
        public (double, double, double, double) GetWallPositions()
        {
            double n = this.CornerNW.y;
            double w = this.CornerNW.x;
            double s = this.CornerSE.y;
            double e = this.CornerSE.x;

            return (n, e, s, w);
        }

        public (double, double) GetRoomCenter()
        {
            double x = (this._wallPos.e + this._wallPos.w) / 2;
            double y = (this._wallPos.n + this._wallPos.s) / 2;

            return (x, y);
        }
    }
}
