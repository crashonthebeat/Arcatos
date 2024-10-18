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
        public Scene CurrentScene { get; private set; }
        public int checkScore;
        public Box HeldItems { get; private set; }

        public Player(Scene currentScene)
        {
            this.CurrentScene = currentScene;
            this.HeldItems = new Box(this, BoxType.Int);
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
                case string s when (new string[] { "look", "view", "examine", "gander" }).Contains(s):
                    this.LookAt(command);
                    return true;
                case string s when (new string[] { "go", "move", "walk", "mosey" }).Contains(s):
                    this.Move(command);
                    return true;
                case string s when (new string[] { "use" }).Contains(s):
                    this.UseItem(command);
                    return true;
                case string s when (new string[] { "get", "take", "grab", "yoink" }).Contains(s):
                    this.GetItem(command);
                    return true;
                case string s when (new string[] { "drop", "toss", "yeet" }).Contains(s):
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
            // If dirobj is "room"
            if (command.DirObj == "room" || this.CurrentScene.Name.Contains(command.DirObj))
            {
                this.CurrentScene.Enter();
                return true;
            }
            else if ((new string[] {"self", "me"}).Contains(command.DirObj))
            {
                this.Examine();
                return true;
            }

            // FindItem
            (Item? item, Box? _) = FindItem(command.DirObj, Game.Boxscope.Local);
            if (item != null)
            {
                item.Examine();
                // Use the item
                return true;
            }

            // FindNPC

            return false;
        }

        public bool Move(Command command)
        {
            // TryGetValue??
            if (!this.CurrentScene.Exits.ContainsKey(command.DirObj))
            {
                Game.Narrate("You cannot go that way.");
                return false; 
                
            }
            else if (this.CurrentScene.Exits[command.DirObj].IsClosed)
            {
                Game.Narrate($"{this.CurrentScene.Exits[command.DirObj].summary} is closed");
                return false;
            }

            Exit exit = this.CurrentScene.Exits[command.DirObj];
            Scene nextRoom = exit.Adjacencies[this.CurrentScene];

            Game.Narrate($"You {command.Action} {command.DirObj}.");
            this.CurrentScene = nextRoom;
            this.CurrentScene.Enter();
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

        public (Item?, Box?) FindItem(string search, List<Box> scope)
        {
            bool found = false;
            Item foundItem = null;
            Box foundBox = null;
            
            // Loop through all the boxes in scope, call FindItem on each box.
            foreach (Box box in scope)
            {
                List<Item> foundItems = box.FindItem(search);

                // Case if FindItem found a single item, and the item has not been found.
                if (foundItems != null && foundItems.Count == 1 && !found)
                {
                    found = true;
                    foundItem = foundItems[0];
                    foundBox = box;
                    foreach (Item item in foundItems)
                    {
                        Dev.Log($"* Found {item.ToString()}");
                    }
                }
                // Case if items were found but there were multiple matches or a match has already been found.
                else if (foundItems != null && (found || foundItems.Count > 1))
                {
                    Game.Narrate($"Which {search} do you mean?");
                    return (null, null);
                }
                else
                {
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
