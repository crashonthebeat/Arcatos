using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Arcatos.Types.Models
{
    public class DoorModel : EntityModel
    {
        [JsonInclude] public required string[] scenes;
        [JsonInclude] public bool closed;
        [JsonInclude] public bool locked;
        [JsonInclude] public bool hidden;
    }
}
