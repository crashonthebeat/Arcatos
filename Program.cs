using System.Text.Json;
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


            StartGame();
        }
    }
}
