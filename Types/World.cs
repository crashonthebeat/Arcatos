using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arcatos.Types
{
    // Data Struct for Json
    public struct WorldDto
    {
        [JsonInclude] public required string worldName;
        [JsonInclude] public required string[] mapNames;
    }
    
    public class World
    {
        public string WorldId { get; init; }
        public Dictionary<string, Map> Maps { get; init; }

        public World(string id)
        {
            this.WorldId = id;
            this.Maps = LoadMaps();
        }

        // LoadMaps initializes the world. This should be called as many times during initial load as there are adjacent worlds to the player.
        // When player moves to a new world it should discard any non-adjacent worlds and then add any new adjacent ones.
        private Dictionary<string, Map> LoadMaps()
        {
            // Empty Dict
            Dictionary<string, Map> loadedMaps = new Dictionary<string, Map>();
            
            // Load Data Object
            using FileStream json = File.OpenRead(Path.Combine(Program.Dir, "World", this.WorldId + ".json"));
            WorldDto dto = JsonSerializer.Deserialize<WorldDto>(json)!;
            
            // Run through map IDs
            foreach (string mapName in dto.mapNames)
            {
                // Construct path to map file
                string path = Path.Combine(Program.Dir, "World", "Maps", this.WorldId, mapName + ".json");
                
                // Construct Map ID
                string mapId = $"{this.WorldId}_{mapName}";

                // Load Data Object for map and create new object.
                using FileStream mapJson = File.OpenRead(path);
                MapDto model = JsonSerializer.Deserialize<MapDto>(mapJson)!;
                loadedMaps[mapId] = new Map(model, mapId);
            }

            return loadedMaps;
        }

        // GetMapObjects is a way to get a Scene or Map from the World object.
        public (Map, Scene) GetMapObjects(string sceneId)
        {
            string[] s = sceneId.Split('_');
            string mapid = $"{this.WorldId}_{s[1]}";

            return (this.Maps[mapid], this.Maps[mapid].Scenes[sceneId]);
        }
    }
}