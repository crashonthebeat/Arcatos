using Arcatos.Types;
using Arcatos.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Arcatos.Types.Interfaces;
using System.Net;
using System.Collections;

namespace Arcatos
{
    public class Player : IEntity
    {
        public string Id           { get => "player"; }
        public Scene  CurrentScene { get; private set; }
        public Box    HeldItems    { get; }

        public Player(Scene currentScene)
        {
            this.CurrentScene = currentScene;
            this.HeldItems    = new Box(this, BoxType.Int);
        }

        public bool Execute(Command command)
        {
            string[] lookVerbs = ["look", "view", "examine", "gander"];
            string[] moveVerbs = ["go", "move", "walk", "mosey"];
            string[] useVerbs = ["use"];
            string[] getVerbs = ["get", "take", "grab", "yoink"];
            string[] dropVerbs = ["drop", "toss", "yeet"];

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
                case "quit":
                    return false;
                default:
                    Game.Narrate($"You do not know how to {command.Action}.");
                    return true;
            }
        }

        #region Basic Interactions

        public bool LookAt(Command command)
        {
            string[] fstPsnPronouns = ["self", "me"];
            
            // If dirobj is "room"
            if (command.DirObj == "room" || this.CurrentScene.Name.Contains(command.DirObj))
            {
                this.CurrentScene.Enter();
                return true;
            }
            else if (fstPsnPronouns.Contains(command.DirObj))
            {
                this.Examine();
                return true;
            }

            // FindItem
            (Item? item, Box? _) = this.FindItem(command.DirObj, Game.Boxscope.Local);
            if (item == null) return false;
            item.Examine();
            // Use the item
            return true;

            // FindNPC

        }

        public bool Move(Command command)
        {
            // TryGetValue??
            if (!this.CurrentScene.Exits.ContainsKey((Dir)Enum.Parse(typeof(Dir), command.DirObj)))
            {
                Game.Narrate("You cannot go that way.");
                return false; 
                
            }

            Dir dir = (Dir)Enum.Parse(typeof(Dir), command.DirObj);

            Exit exit = this.CurrentScene.Exits[dir];
            Scene nextRoom = exit.AdjScenes[this.CurrentScene];

            Game.Narrate($"You {command.Action} {command.DirObj}.");
            this.CurrentScene = nextRoom;
            this.CurrentScene.Enter();
            Boxscope.UpdateLocal();
            return true;
        }

        public bool UseItem(Command command)
        {
            (Item? item, Box? _) = FindItem(command.DirObj, Game.Boxscope.Local);

            if (item != null)
            {
                Game.Narrate($"You use {item.Name}.");
                // Use the item
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
        #region Inventory Interactions

        public bool GetItem(Command command)
        {
            (Item? item, Box? box) = FindItem(command.DirObj, Game.Boxscope.Scene);

            if (item != null)
            {
                box.RemoveItem(item);
                this.HeldItems.AddItem(item);
                Game.Narrate($"You {command.Action} {item.Glance()}.");
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DropItem(Command command)
        {
            (Item? item, Box? box) = FindItem(command.DirObj, Game.Boxscope.Player);

            if (item != null && box == this.HeldItems)
            {
                Game.Narrate($"You {command.Action} {item.Glance()} on the floor.");
                this.HeldItems.RemoveItem(item);
                return true;
            }
            else if (item != null)
            {
                Game.Narrate([$"You are not holding {item.Glance()}"]);
            }

            return false;
        }
    
        // TODO: Broken - not finding item despite one match found.
        // TODO: Also should probably get command parsing to take an input with only an indObj to make it a dirObj
        public (Item?, Box?) FindItem(string search, List<Box> scope)
        {
            //bool found = false;
            Item? foundItem = null;
            Box? foundBox = null;
            
            // Loop through all the boxes in scope, call FindItem on each box.
            foreach (Box box in scope)
            {
                List<Item>? foundItems = box.FindItem(search);
                
                // Check if items are found:

                switch (foundItems)
                {
                    // Case if FindItem found a single item and nothing has been found yet.
                    case { Count: 1 } when foundItem == null:
                    {
                        //found = true;
                        foundItem = foundItems[0];
                        foundBox = box;
                        Dev.Log($"* Found {foundItem.ToString()}");
                        break;
                    }
                    // When the box has a matching item, but an item has been found.
                    case { Count: 1 }:
                        Dev.Log($"* Found {foundItem.ToString()}");
                        Game.Narrate($"Which {search} do you mean?");
                        return (null, null);
                    // Case if items were found but there were multiple matches or a match has already been found.
                    case { Count: > 1 }:
                        Game.Narrate($"Which {search} do you mean?");
                        return (null, null);
                    default:
                        continue;
                }
            }

            return (foundItem, foundBox);
        }

        #endregion
        #region Inherited Methods

        public void Examine()
        {
            Game.Narrate("You see an amazing mortal being");
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
