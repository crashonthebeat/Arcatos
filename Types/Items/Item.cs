using System.Text.Json.Serialization;

namespace Arcatos.Types.Items
{
    public struct ItemDto
    {
        [JsonInclude] public          string   name;
        [JsonInclude] public required string   summary;
        [JsonInclude] public required string[] description;
        [JsonInclude] public          bool     isConsumable;
    }
    
    public class Item : Entity
    {
        public int  Size         { get; init; }
        public int  Weight       { get; init; } // Weight in grams
        public bool IsConsumable { get; init; }
        
        public Item (string id, ItemDto dto) : base(id, dto.summary, dto.description, dto.name)
        {
            this.IsConsumable = dto.isConsumable;
        }
        
        public Item (string id, EntityDto dto) : base(id, dto.Summary, dto.Description, dto.Name!)
        {
            this.IsConsumable = dto.IsConsumable ?? false;
        }

        public Item (string id, string name, string summary, string[] desc) : base(id, summary, desc, name)
        {

        }
    }
}
