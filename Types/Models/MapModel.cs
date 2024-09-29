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
    internal class MapModel
    {
        [JsonInclude] public required SceneModel[] scenes;
        [JsonInclude] public required DoorModel[] exits;

        public Map ToDomainModel()
        {
            return new Map(this.scenes, this.exits);
        }
    }
}
