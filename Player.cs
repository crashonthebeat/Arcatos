using Arcatos.Types;
using Arcatos.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos
{
    public class Player
    {
        public Scene CurrentScene { get; private set; }
        public int checkScore;

        public Player(Scene currentScene)
        {
            this.CurrentScene = currentScene;
        }

        public bool Execute(Command command)
        {            
            switch (command.Action)
            {
                case "look": case "view": case "examine":
                    this.LookAt(command);
                    return true;
                case "go": case "move": case "walk": case "mosey":
                    this.Move(command);
                    return true;
                case "use":
                    this.UseItem(command);
                    return true;
                case "quit":
                    return false;
                default:
                    Game.Narrate([$"You do not know how to {command.Action}."]);
                    return true;
            }
        }

        public bool LookAt(Command command)
        {
            // If dirobj is "room"
            if (command.DirObj == "room" || this.CurrentScene.Name.Contains(command.DirObj))
            {
                this.CurrentScene.Enter();
                return true;
            }

            // FindItem
            Item? item = Find(command.DirObj);
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
                Game.Narrate(["You cannot go that way."]);
                return false; 
                
            }
            else if (this.CurrentScene.Exits[command.DirObj].isClosed)
            {
                Game.Narrate([$"{this.CurrentScene.Exits[command.DirObj].summary} is closed"]);
                return false;
            }

            Door exit = this.CurrentScene.Exits[command.DirObj];
            Scene nextRoom = exit.Adjacencies[this.CurrentScene];

            Game.Narrate([$"You {command.Action} {command.DirObj}."]);
            this.CurrentScene = nextRoom;
            this.CurrentScene.Enter();
            return true;
        }

        public bool UseItem(Command command)
        {
            Item? item = Find(command.DirObj);

            if (item != null)
            {
                Game.Narrate([$"You use {item.Name}."]);
                // Use the item
                return true;
            }
            else
            {
                return false;
            }
        }

        public Item? Find(string searchString)
        {
            List<Item> results = this.CurrentScene.Inventory.FindItem(searchString);

            if (results.Count == 0)
            {
                Game.Narrate([$"You cannot find {searchString}."]);
                return null;
            }
            else if (results.Count > 1)
            {
                Game.Narrate([$"Which {searchString} do you mean?"]);
                return null;
            }
            else
            {
                return results[0];
            }
        }
    }
}
