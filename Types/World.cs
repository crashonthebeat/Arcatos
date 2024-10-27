using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Arcatos.Utils;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

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
        private string _path;
        
        public string WorldId { get; init; }
        public Dictionary<string, Map> Maps { get; init; }

        public World(string id)
        {
            this.WorldId = id;
            this._path   = Path.Combine(Program.Dir, "World", "Maps", this.WorldId);
            this.Maps    = this.LoadMaps();
        }

        // LoadMaps initializes the world. This should be called as many times during initial load as there are adjacent worlds to the player.
        // When player moves to a new world it should discard any non-adjacent worlds and then add any new adjacent ones.
        private Dictionary<string, Map> LoadMaps()
        {
            Dev.Log($"Loading {this.WorldId}\n################################################");
            // Empty Dict
            Dictionary<string, Map> loadedMaps = new Dictionary<string, Map>();
            
            // Load Data Object
            using FileStream json = File.OpenRead(Path.Combine(Program.Dir, "World", this.WorldId + ".json"));
            WorldDto         dto  = JsonSerializer.Deserialize<WorldDto>(json)!;
            
            // Run through map IDs
            foreach (string mapName in dto.mapNames)
            {
                Dev.Log($"Loading {mapName}");
                // Construct path to map file
                string data = Path.Combine(this._path, mapName + ".json");
                
                // Construct Map ID
                string mapId = $"{this.WorldId}_{mapName}";
                Dev.Log($"Loading {mapId}");
                
                // Load all layouts to pass to map constructor
                Dictionary<string, LayoutDto> layouts = this.LoadLayouts(mapName);

                // Load Data Object for map and create new object.
                using FileStream mapJson = File.OpenRead(data);
                MapDto model = JsonSerializer.Deserialize<MapDto>(mapJson)!;
                loadedMaps[mapId] = new Map(model, layouts, mapId);
            }

            return loadedMaps;
        }

        // I am so proud of this it's insane.
        // This takes an xml file with room definitions and creates a layout dto that adds coordinates to the rooms
        // Why is this here? BECAUSE THE XML FILE WAS GENERATED FROM A DIAGRAM CREATED IN DRAW.IO
        // So now. You can design something in draw.io, export it to xml, save it to the engine with the right name and THE ROOMS GET PLACED
        private Dictionary<string, LayoutDto> LoadLayouts(string mapName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(this._path, mapName + "_layout.xml"));

            XmlNodeList? roomDefs = doc.FirstChild?.NextSibling?.FirstChild?.FirstChild?.FirstChild?.ChildNodes;
            
            Dictionary<string, LayoutDto> layouts = new Dictionary<string, LayoutDto>();

            if (roomDefs == null) throw new WeastException($"Layout document {mapName}_layout.xml could not be parsed.");
            
            foreach (XmlNode node in roomDefs)
            {
                // Grab all relevant attributes
                if (node.Attributes?["id"]?.Value is "0" or "1") continue;
                string? id = node.Attributes?["value"]?.Value;
                if (id is null) continue;
                
                string? xStr = node.FirstChild?.Attributes?["x"]?.Value;
                Dev.Log(id);
                int x = xStr != null ? Convert.ToInt32(xStr) : 0;
                
                string? yStr = node.FirstChild?.Attributes?["y"]?.Value;
                int     y    = yStr != null ? Convert.ToInt32(yStr) : 0;
                
                string? widthStr = node.FirstChild?.Attributes?["width"]?.Value;
                int     width    = widthStr != null ? Convert.ToInt32(widthStr) : 0;
                
                string? heightStr = node.FirstChild?.Attributes?["height"]?.Value;
                int     height    = heightStr != null ? Convert.ToInt32(heightStr) : 0;
                
                layouts.Add(id, new LayoutDto(x, y, width, height));
            }
            
            return layouts;
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