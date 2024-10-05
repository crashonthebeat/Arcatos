using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Arcatos.Types.Models
{
    public class MapModel
    {
        [JsonInclude] public required SceneModel[] scenes;
        [JsonInclude] public required DoorModel[] exits;
        [JsonInclude] public required Dictionary<string, Dictionary<string, int>> inventories;

        public Map ToDomainModel()
        {
            return new Map(this.scenes, this.exits, this.inventories);
        }
    }
}
