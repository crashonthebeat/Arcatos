﻿using Arcatos.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arcatos.Types
{
    // Data Object for the Map Type
    public struct MapDto
    {
        [JsonInclude] public required SceneDto[] scenes;
        [JsonInclude] public required ExitDto[] exits;
        [JsonInclude] public required Dictionary<string, Dictionary<string, int>> inventories;
    }
    
    public class Map
    {
        public Dictionary<string,Scene> Scenes { get; init; }   // Dict of scenes with their id's as key.
        public Dictionary<string,Exit> Exits { get; init; }     // Dict of exits with their id's as key.

        
        // Map Constructor is called by the world's loading function and takes the Dto generated by the map json.
        public Map(MapDto dto, string mapName)
        {
            this.Scenes = new Dictionary<string, Scene>();
            this.Exits = new Dictionary<string, Exit>();

            // Process scenes array in MapDto
            foreach (SceneDto model in dto.scenes)
            {
                // Array Item is SceneDto > Process to Scene
                Scene scene = new Scene(model);

                // Add processed scene under its id.
                this.Scenes.Add( $"{mapName}_{scene.id}", scene );
            }

            // TODO: Redo Exit processing.
            // Process Exits array
            foreach (ExitDto model in dto.exits)
            {
                // Grab Scenes from the id names in the ExitDto and create new Door Object.
                Scene[] adj = [this.Scenes[$"{mapName}_{model.scenes[0]}"], this.Scenes[$"{mapName}_{model.scenes[1]}"]];
                this.Exits.Add(model.id, new Exit(model, adj));

                // Add exit to each scene in its adjacencies.
                foreach (Scene scene in adj)
                {
                    // Finally, add all adjacencies (construct the layout).
                    scene.AddExit(Exits[model.id]);
                }
            }

            // Process Inventories
            foreach (string invId in dto.inventories.Keys)
            {
                string id = $"{mapName}_{invId}";

                Scene scene = this.Scenes[id];
                Dictionary<string, int> itemDefs = dto.inventories[invId];

                scene.LoadItems(itemDefs);
            }
        }
    }
}
