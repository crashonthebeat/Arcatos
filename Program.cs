using System.Runtime.CompilerServices;
using System.Text.Json;
using Arcatos.Utils;

namespace Arcatos
{
    static class Program
    {
        public static Settings Settings { get; set; }   // Program Settings

        // Program Initialization
        static Program()
        {
            using FileStream json = File.OpenRead("EngineSettings.json");
            Settings = JsonSerializer.Deserialize<Settings>(json)!;
        }

        static void Main(string[] args)
        {
            Dev.Log("Game Initialized");
        }
    }
}
