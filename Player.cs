using Arcatos.Types;
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

        public Player(Scene currentScene)
        {
            this.CurrentScene = currentScene;
        }
    }
}
