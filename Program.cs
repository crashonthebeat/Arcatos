using System.Text.Json;
using Arcatos.Engine;
using Arcatos.Utils;

namespace Arcatos
{
    internal static class Program
    {
        public static          Settings Settings { get; set; }   // Program Settings
        public static          Game?    Game     { get; set; }
        public static readonly string   Dir   = Environment.CurrentDirectory;

        // Program Initialization
        static Program()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Initializing Program");
            Console.ResetColor();

            using FileStream json = File.OpenRead("EngineSettings.json");
            Program.Settings           = JsonSerializer.Deserialize<Settings>(json)!;
            Program.Settings.DebugMode = true;
        }

        private static void StartGame()
        {
            Dev.Log("Game Initialized");
            
            // This line will be called from a loaded player state or new game state.
            Game = new Game("caranae_keepL1_main_hall");
            
            while (Game.Playing)
            {
                Game.Play();
            }
        }

        static void Main(string[] args)
        {
            //Item test = new Item("blag", "bag", "dfsdfs", ["tesdt"]);
            //Type blah = test.GetType();

            StartGame();
        }

        public static Dictionary<string, T> LoadFiles<T>(string path)
        {
            string[] files = Directory.GetFiles(Path.Combine(Program.Dir, Path.Combine(path.Split('/'))));
            
            Dictionary<string,T> objs = new Dictionary<string, T>();


            //StartGame();
            foreach (string file in files)
            {
                using FileStream json = File.OpenRead(file);
                foreach (KeyValuePair<string, T> obj in JsonSerializer.Deserialize<Dictionary<string, T>>(json)!)
                {
                    objs.Add(obj.Key, obj.Value);
                }
            }

            return objs;
        }

        public static T? LoadFile<T>(string path)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            using StreamReader streamReader = new(path);
            var json = streamReader.ReadToEnd();
            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}
