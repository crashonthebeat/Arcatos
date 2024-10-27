using System.Text.Json.Serialization;

namespace Arcatos.Types.Items
{
    public struct ItemDto
    {
        [JsonInclude] public          string   name;
        [JsonInclude] public required string   summary;
        [JsonInclude] public required string[] desc;
        [JsonInclude] public          bool     isConsumable;
    }
    
    public class Item : Entity
    {
        public bool IsConsumable { get; }
        
        public Item (string id, ItemDto dto) : base(id, dto.summary, dto.desc, dto.name)
        {
            this.IsConsumable = dto.isConsumable;
        }

        public Item (string id, string name, string summary, string[] desc) : base(id, summary, desc, name)
        {

        }
    }
}
