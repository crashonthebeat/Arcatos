﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arcatos.Types.Models
{
    public class SceneModel : EntityModel
    {
        [JsonInclude] public required int[] coords;
        [JsonInclude] public bool visited;
    }
}
