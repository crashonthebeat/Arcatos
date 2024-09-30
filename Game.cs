using Arcatos.Types;
using Arcatos.Types.Models;
using Arcatos.Utils;
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
        public bool Playing { get; set; }
        public Map CurrentMap { get; set; }
        public Player Player { get; set; }

        public Game(string mapname)
        {
            this.CurrentMap = LoadMap(mapname);
            this.Player = new Player(this.CurrentMap.Scenes["testscene_1"]);
            this.Playing = true;
        }

        public bool Play()
        {
            this.Player.CurrentScene.Enter(this.Player);

            return Prompt();
        }

        public bool Prompt()
        {
            while (Playing)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (String.IsNullOrEmpty(input)) continue;

                Command command = new Command(input.ToLower().Split(' '));

                this.Playing = this.Player.ProcessCommand(command);
            }
            return true;
        }

        public static Map LoadMap(string mapname)
        {
            using FileStream json = File.OpenRead(Path.Combine("World", mapname + ".json"));
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
