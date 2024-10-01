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
        public static Map CurrentMap { get; set; }
        public static Player Player { get; set; }

        public Game(string mapname)
        {
            Game.CurrentMap = LoadMap(mapname);
            Game.Player = new Player(Game.CurrentMap.Scenes["testscene_1"]);
            this.Playing = true;
        }

        public bool Play()
        {
            Game.Player.CurrentScene.Enter();

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

                this.Playing = Game.Player.ProcessCommand(command);
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
