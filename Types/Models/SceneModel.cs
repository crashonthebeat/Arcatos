using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arcatos.Types.Models
{
    internal class SceneModel : EntityModel
    {
        [JsonInclude] public int[] coords;
        [JsonInclude] public bool visited;
    }
}
