using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Windows;
using Arcatos.Types;
using Arcatos.Utils;

namespace Arcatos
{
    static class Program
    {
        public static Settings Settings { get; set; }   // Program Settings
        public static Game? Game { get; set; }
        public static string Dir = Environment.CurrentDirectory;

        // Program Initialization
        static Program()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Initializing Program");
            Console.ResetColor();

            using FileStream json = File.OpenRead("EngineSettings.json");
            Settings = JsonSerializer.Deserialize<Settings>(json)!;
            Settings.DebugMode = true;
        }

        static void Main(string[] args)
        {
            Dev.Log("Game Initialized");

            // This line will be called from a loaded player state or new game state.
            Game = new("caranae_keepL1_main_hall");

            while (Game.Playing)
            {
               Game.Play();
            }
        }
    }
}
