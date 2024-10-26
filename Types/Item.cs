using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Arcatos.Types
{
    public struct ItemDto
    {
        [JsonInclude] public          string   name;
        [JsonInclude] public required string   summary;
        [JsonInclude] public required string[] desc;
        [JsonInclude] public          bool     isUnique;
    }
    
    public class Item : Entity
    {
        public Item (string id, ItemDto dto) : base(id, dto.summary, dto.desc, dto.name)
        {
            
        }

        public Item (string id, string name, string summary, string[] desc) : base(id, summary, desc, name)
        {

        }
    }
}
