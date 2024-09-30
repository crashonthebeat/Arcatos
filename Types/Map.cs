using Arcatos.Types.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arcatos.Types
{
    public class Map
    {
        public Dictionary<string,Scene> Scenes { get; init; }   // Dict of scenes with their id's as key.
        public Dictionary<string,Door> Exits { get; init; }     // Dict of exits with their id's as key.
        //public char[][] MapDisplay { get; set; }

        
        // Map Constructor is called by the loading function and is the scene list and exit list from the mapmodel
        // All scenes and exits are constructed as objects attached to their ID as their keys.
        public Map(SceneModel[] scenes, DoorModel[] exits)
        {
            // Create new Dictionaries
            this.Scenes = new Dictionary<string, Scene>();
            this.Exits = new Dictionary<string, Door>();

            // Process all SceneModels in MapModel using their parameters to create new Scene objects.
            foreach (SceneModel scene in scenes)
            {
                Scenes.Add(scene.id, new Scene(scene.id, scene.name, scene.summary, scene.desc, scene.coords));
            }

            // Process all DoorModels in MapModel using their parameters to create new exits.
            foreach (DoorModel exit in exits)
            {
                // Get the scene objects from the Scene dict based on the ids in the DoorModel.
                Scene[] adj = [this.Scenes[exit.scenes[0]], this.Scenes[exit.scenes[1]]];

                Exits.Add(exit.id, new Door(exit.id, exit.summary, exit.desc, adj, exit.closed, exit.locked, exit.hidden));

                foreach (Scene scene in adj)
                {
                    // Finally, add all adjacencies (construct the layout).
                    scene.AddExit(Exits[exit.id]);
                }
            }
        }
    }
}
