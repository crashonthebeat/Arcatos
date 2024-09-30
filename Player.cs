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

        public bool ProcessCommand(Command command)
        {
            switch (command.Action)
            {
                case "go": case "move": case "walk":
                    this.Move(command.DirObj);
                    return true;
                case "quit":
                    return false;
                default:
                    return true;
            }
        }

        public bool Move(string direction)
        {
            // TryGetValue??
            if (!this.CurrentScene.Exits.ContainsKey(direction))
            {
                return false; 
                
            }
            else if (this.CurrentScene.Exits[direction].isClosed)
            {
                return false;
            }

            Door exit = this.CurrentScene.Exits[direction];
            Scene nextRoom = exit.Adjacencies[this.CurrentScene];

            this.CurrentScene = nextRoom;
            this.CurrentScene.Enter(this);
            return true;
        }
    }
}
