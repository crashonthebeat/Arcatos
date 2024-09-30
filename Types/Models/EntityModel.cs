using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Arcatos.Types.Models
{
    public class EntityModel
    {
        [JsonInclude] public required string id;
        [JsonInclude] public required string name;
        [JsonInclude] public required string summary;
        [JsonInclude] public required string[] desc;
    }
}
