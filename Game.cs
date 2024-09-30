using Arcatos.Types;
using Arcatos.Types.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Arcatos
{
    public class Game
    {        
        public Map CurrentMap { get; set; }
        public Player Player { get; set; }

        public Game(string mapname)
        {
            this.CurrentMap = LoadMap(mapname);
            this.Player = new Player(this.CurrentMap.Scenes["testscene_1"]);
        }

        public void Play()
        {
            this.Player.CurrentScene.Enter(3);
        }
        
        public static Map LoadMap(string mapname)
        {
            using FileStream json = File.OpenRead(@"World\" + mapname + ".json");
            MapModel model = JsonSerializer.Deserialize<MapModel>(json)!;

            return model.ToDomainModel();
        }
        
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
