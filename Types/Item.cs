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
        [JsonInclude] public required string id;
        [JsonInclude] public string name;
        [JsonInclude] public required string summary;
        [JsonInclude] public required string[] desc;
    }
    
    public class Item : Entity
    {
        public override Box? Inventory{ get; set; }

        public Item (ItemDto dto) : base(dto.id, dto.summary, dto.desc, dto.name)
        {
            
        }

        public Item (string id, string name, string summary, string[] desc) : base(id, summary, desc, name)
        {

        }
    }
}
