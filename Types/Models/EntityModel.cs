using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Arcatos.Types.Models
{
    internal class EntityModel
    {
        [JsonInclude] public string id;
        [JsonInclude] public string name;
        [JsonInclude] public string summary;
        [JsonInclude] public string[] desc;
    }
}
