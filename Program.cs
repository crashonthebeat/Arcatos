using System.Text.Json;
using Arcatos.Engine;
using Arcatos.Utils;

namespace Arcatos
{
    internal static class Program
    {
        public static          Settings Settings { get; set; }   // Program Settings
        public static          Game?    Game     { get; set; }
        public static readonly string   Dir          = Environment.CurrentDirectory;
        private const          string   SettingsFile = "EngineSettings.json";

        // Program Initialization
        static Program()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Loading Program Settings...");
            
            using FileStream json = File.OpenRead(Program.SettingsFile);
            Program.Settings           = JsonSerializer.Deserialize<Settings>(json)!;
            Program.Settings.DebugMode = true;
            
            
            Dev.Log("Initializing Program");
            Console.ResetColor();
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

        static void Main()
        {
            //Item test = new Item("blag", "bag", "dfsdfs", ["tesdt"]);
            //Type blah = test.GetType();

            StartGame();
        }
    }
}
