using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos
{
    public class Game
    {        
        // Narrate is a wrapper on top of console.write that takes game narration and presents it to the player.
        public static void Narrate(string[] raw)
        {
            string joined = "";
            
            foreach (string s in raw)
            {
                joined += $"{s.Trim()} ";
            }

            Console.WriteLine(joined);

            // Todo: Wrap strings based on a detected (or set or standard) console size.
        }
    }
}
