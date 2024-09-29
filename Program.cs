using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Windows;
using Arcatos.Types;
using Arcatos.Types.Models;
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

            // Temporary Load World
            using FileStream json = File.OpenRead(@"World\TestWorld.json");
            MapModel test = JsonSerializer.Deserialize<MapModel>(json)!;

            Map world = test.ToDomainModel();

            world.Scenes["testscene_1"].Enter(3);
        }
    }
}
