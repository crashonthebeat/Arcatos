using Arcatos.Types;
using Arcatos.Types.Interfaces;
using Arcatos.Types.Items;
using Arcatos.Utils;

namespace Arcatos.Engine
{
    public class Player : IEntity
    {
        public string Id            { get => "player"; }
        public Scene  CurrentScene  { get; private set; }
        public Box    HeldItems     { get; }
        public Box    EquippedItems { get; }

        public Dictionary<EqSlot, List<Equipment>> Equipment { get; set; } = new Dictionary<EqSlot, List<Equipment>>();

        private const bool   Debug   = false;

        public Player(Scene currentScene)
        {
            this.CurrentScene  = currentScene;
            this.HeldItems     = new Box(this, BoxType.Int);
            this.EquippedItems = new Box(this, BoxType.Ext);
        }

        
        public bool Execute(Command command)
        {
            switch (command.Action)
            {
                case "look": case "view": case "examine": case "gander":
                    this.LookAt(command); return true;
                case "go": case "move": case "walk": case "mosey":
                    this.Move(command); return true;
                case "use":
                    this.UseItem(command); return true;
                case "get": case "take": case "grab": case "yoink": case "pickup":
                    this.GetItem(command); return true;
                case "drop": case "toss": case "yeet": case "discard":
                    this.DropItem(command); return true;
                case "equip": case "wear":
                    this.Equip(command); return true;
                case "unequip": case "remove": case "undress":
                    this.Unequip(command); return true;
                case "unlock":
                    this.Unlock(command); return true;
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
            
            // TODO: Update Boxscope if item is container
        }

        private void DropItem(Command command)
        {
            (Item? item, Box? box) = Player.FindItem(command.DirObj, Game.Boxscope.Player);

            if (item != null && box == this.HeldItems)
            {
                Game.Write($"You {command.Action} {item.Glance()} on the floor.");
                this.HeldItems.RemoveItem(item);
                this.CurrentScene.Inventory.AddItem(item);
                // TODO: Update Boxscope if item is container
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
            }

            // Cast to Equipment type
            Equipment eqItem = item as Equipment ?? throw new NullReferenceException($"Item is not an equipment");

            // Check if slot does not exist on player already
            if (!this.Equipment.ContainsKey(eqItem.Slot))
            {
                // Create slot on player and add equipment item
                this.Equipment[eqItem.Slot] = [  ];
            }
            // Check if the top item has a higher layer than the equipment you are trying to wear
            else if (this.Equipment[eqItem.Slot][-1].Layer >= eqItem.Layer)
            {
                Game.Write("That won't fit over the top of what you are currently wearing!");
                return;
            }
            // Add to Equipment Slot
            this.Equipment[eqItem.Slot].Add(eqItem);
            this.EquippedItems.AddItem(eqItem);
            this.HeldItems.RemoveItem(item);
            Game.Write($"You put on the {command.DirObj}.");
            
            // TODO: Update Boxscope if item is container
        }

        private void Unequip(Command command)
        {
            List<Item> found = this.EquippedItems.FindItem(command.DirObj);
            if (found.Count > 1)
            {
                Game.Write(Narrator.Player("ManyItemResults", command.DirObj));
            }
            else if (found.Count == 0)
            {
                // TODO: Make this into a new narration list in the data folder
                Game.Write(Narrator.Player("NoHeldItem", command.DirObj));
            }
            // Cast found item to equipment
            Equipment eqItem = found[0] as Equipment ?? throw new NullReferenceException($"{found[0].Name} is not an equipment");
            
            // Remove item from Equipped Items container and move to HeldItems.
            this.EquippedItems.RemoveItem(eqItem);
            this.HeldItems.AddItem(eqItem);
            
            // Remove item from equipped items
            this.Equipment[eqItem.Slot].Remove(eqItem);
            if (this.Equipment[eqItem.Slot].Count == 0)
            {
                this.Equipment.Remove(eqItem.Slot);
            }

            // TODO: Update Boxscope if item is container
        }
        
        #endregion
        #region Inherited Methods

        public void Examine()
        {
            Game.Write("You see an amazing mortal being");

            if (this.EquippedItems.Items.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Game.Write("You are wearing: ");
                Console.ResetColor();
            }
            
            // List Equipment
            foreach (var kvp in this.Equipment)
            {
                string s = $"{Game.Titleize(kvp.Key.ToString())}: ";
                foreach (Equipment eqItem in kvp.Value)
                {
                    s += $"{eqItem.Glance()} ";
                }

                Game.Write(s);
            }
            
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
