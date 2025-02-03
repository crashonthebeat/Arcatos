using Arcatos.Types;
using Arcatos.Types.Interfaces;
using Arcatos.Types.Items;
using Arcatos.Utils;

namespace Arcatos.Engine
{
    public class Player : IEntity
    {
        public string Id           { get => "player"; }
        public Scene  CurrentScene { get; private set; }
        public Box    HeldItems    { get; }

        public Dictionary<EqSlot, Item[]> Equipment { get; set; } = new Dictionary<EqSlot, Item[]>();

        private const bool   Debug   = false;

        public Player(Scene currentScene)
        {
            this.CurrentScene = currentScene;
            this.HeldItems    = new Box(this, BoxType.Int);
        }

        public bool Execute(Command command)
        {
            string[] lookVerbs   = ["look", "view", "examine", "gander"];
            string[] moveVerbs   = ["go", "move", "walk", "mosey"];
            string[] useVerbs    = ["use"];
            string[] getVerbs    = ["get", "take", "grab", "yoink"];
            string[] dropVerbs   = ["drop", "toss", "yeet"];
            string[] unlockVerbs = ["unlock"];

            switch (command.Action)
            {
                case { } s when lookVerbs.Contains(s):
                    this.LookAt(command);
                    return true;
                case { } s when moveVerbs.Contains(s):
                    this.Move(command);
                    return true;
                case { } s when useVerbs.Contains(s):
                    this.UseItem(command);
                    return true;
                case { } s when getVerbs.Contains(s):
                    this.GetItem(command);
                    return true;
                case { } s when dropVerbs.Contains(s):
                    this.DropItem(command);
                    return true;
                case { } s when unlockVerbs.Contains(s):
                    this.Unlock(command);
                    return true;
                case "quit":
                    return false;
                default:
                    Game.Write(Narrator.Player("InvalidAction", command.Action));
                    return true;
            }
        }

        #region Basic Interactions

        private void LookAt(Command command)
        {
            string[] fstPsnPronouns = ["self", "me"];
            
            // If dirobj is "room"
            if (command.DirObj == "room" || this.CurrentScene.Name.Contains(command.DirObj))
            {
                this.CurrentScene.Enter();
                return;
            }
            if (fstPsnPronouns.Contains(command.DirObj))
            {
                this.Examine();
                return;
            }

            // FindItem
            (Item? item, Box? _) = Player.FindItem(command.DirObj, Game.Boxscope.Local);
            if (item == null) return;
            item.Examine();
            // Use the item

            // FindNPC

        }
        
        private void UseItem(Command command)
        {
            (Item? item, Box? _) = Player.FindItem(command.DirObj, Game.Boxscope.Local);

            if (item == null) return;
            Game.Write($"You use {item.Name}.");
            // Use the item
        }

        private static Exit? ValidateDirection(string d)
        {
            if (!Enum.TryParse(d, out Dir dir))
            {
                Dev.Log($"Could not resolve direction {d}.");
                return null;
            }
            Game.Player.CurrentScene.Exits.TryGetValue(dir, out Exit? exit);
            if (exit != null) return exit;
            Dev.Log($"Exit at {d} does not exist.");
            return null;

        }
        
        private void Move(Command command)
        {
            // Exit? exit = !Enum.TryParse(command.DirObj, out Dir dir) 
            //     ? null : this.CurrentScene.Exits.GetValueOrDefault(dir);
            Exit? exit = Player.ValidateDirection(command.DirObj);
            if (exit == null)
            {
                Game.Write($"You cannot go {command.DirObj}.");
                return;
            }

            Scene nextRoom = exit.AdjScenes[this.CurrentScene];

            Game.Write($"You {command.Action} {command.DirObj}.");
            this.CurrentScene = nextRoom;
            this.CurrentScene.Enter();
            Boxscope.UpdateLocal();
        }

        private void Unlock(Command command)
        {
            // Check if a valid direction was entered
            // Exit? exit = !Enum.TryParse(command.DirObj, out Dir dir) 
            //     ? null : this.CurrentScene.Exits.GetValueOrDefault(dir);
            Exit? exit = Player.ValidateDirection(command.DirObj);
            if (exit == null)
            {
                Game.Write("You don't see a door there.");
                return;
            }
            
            // Check if the player specified a key.
            if (command.IndObj == null)
            {
                Game.Write("What are you going to unlock it with, magic? That's not implemented yet.");
                Game.Write("You're lucky I didn't just make this an exception because I think that would be funny");
                return;
            }

            ( Item ? item, _ ) = Player.FindItem(command.DirObj, Game.Boxscope.Player);
            if (item == null)
            {
                Dev.Log("No key found");
                return;
            }

            exit.Unlock(item);
        }

        #endregion
        #region Inventory Interactions

        private void GetItem(Command command)
        {
            (Item? item, Box? box) = Player.FindItem(command.DirObj, Game.Boxscope.Scene);

            if (item == null || box == null)
            {
                Game.Write(Narrator.Player("NoFoundItem", command.DirObj));
                return;
            }
            box.RemoveItem(item);
            this.HeldItems.AddItem(item);
            Game.Write($"You {command.Action} {item.Glance()}.");
            item.Learn();
        }

        private void DropItem(Command command)
        {
            (Item? item, Box? box) = Player.FindItem(command.DirObj, Game.Boxscope.Player);

            if (item != null && box == this.HeldItems)
            {
                Game.Write($"You {command.Action} {item.Glance()} on the floor.");
                this.HeldItems.RemoveItem(item);
                return;
            }
            if (item == null)
            {
                Game.Write(Narrator.Player("NoHeldItem", command.DirObj));
            }
        }
        
        // TODO: Also should probably get command parsing to take an input with only an indObj to make it a dirObj
        public static (Item?, Box?) FindItem(string search, List<Box> scope)
        {
            //bool found = false;
            Item? foundItem = null;
            Box? foundBox = null;
            
            // Loop through all the boxes in scope, call FindItem on each box.
            foreach (Box box in scope)
            {
                List<Item> foundItems = box.FindItem(search);
                
                // Check if items are found:

                switch (foundItems)
                {
                    // Case if FindItem found a single item and nothing has been found yet.
                    case { Count: 1 } when foundItem == null:
                        //found = true;
                        foundItem = foundItems[0];
                        foundBox = box;
                        Dev.Log($"* Found {foundItem}", Player.Debug);
                        break;
                    
                    // When the box has a matching item, but an item has been found.
                    case { Count: 1 }:
                        Dev.Log($"* Found {foundItem}", Player.Debug);
                        Game.Write(Narrator.Player("ManyItemResults", search));
                        return (null, null);
                    // Case if items were found but there were multiple matches or a match has already been found.
                    case { Count: > 1 }:
                        Game.Write(Narrator.Player("ManyItemResults", search));
                        return (null, null);
                    default:
                        continue;
                }
            }

            return (foundItem, foundBox);
        }

        #endregion
        #region Equipment Methods

        private void Equip(Command command)
        {
            // Try to find the equipment
            (Item? item, Box? box) = Player.FindItem(command.DirObj, Game.Boxscope.Player);
            if (item == null || box != this.HeldItems)
            {
                Game.Write(Narrator.Player("NoHeldItem", command.DirObj));
                return;
            }
            if (item is not Types.Items.Equipment)
            {
                Game.Write(Narrator.Player("ItemNotEquipment", item.Glance()));
                return;
            }
            
            // Check for slot validity
            
            // Add to Equipment Slots
        }
        
        #endregion
        #region Inherited Methods

        public void Examine()
        {
            Game.Write("You see an amazing mortal being");
            this.HeldItems.ListItems();
        }

        public string Glance()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Player";
        }

        #endregion
    }
}
