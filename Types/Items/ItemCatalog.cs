using Arcatos.Utils;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml;

namespace Arcatos.Types.Items
{
    public class ItemCatalog
    {
        public Dictionary<string, Item>      All       { get; init; }
        public Dictionary<string, Item>      Generics  { get; init; }
        public Dictionary<string, Item>      Keys      { get; init; }
        public Dictionary<string, Equipment> Equipment { get; init; }
        // public Dictionary<string, Item>      Keys      { get; init; }

        public ItemCatalog()
        {
            string itemsDir = Path.Combine(Program.Dir, "Data", "World", "Items", "common");
            
            this.Generics = new Dictionary<string, Item>();
            foreach ((string? key, EntityDto value) in LoadCatalog(Path.Combine(itemsDir, "mundane.json")))
            {
                this.Generics.Add(key, new Item(key, value));
            }
            
            this.Keys = new Dictionary<string, Item>();
            foreach ((string? key, EntityDto value) in LoadCatalog(Path.Combine(itemsDir, "keys.json")))
            {
                this.Keys.Add(key, new Item(key, value));
            }

            this.Equipment = new Dictionary<string, Equipment>();
            foreach ((string? key, EntityDto value) in LoadCatalog(Path.Combine(itemsDir, "equipment.json")))
            {
                this.Equipment.Add(key, new Equipment(key, value));
            }

            this.All = this.MergeCatalogs();
        }
        
        private static Dictionary<string, EntityDto> LoadCatalog(string fileName)
        {
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            using StreamReader streamReader = new StreamReader(Path.Combine("Items", fileName));
            string             json         = streamReader.ReadToEnd();
            
            return JsonSerializer.Deserialize <Dictionary<string, EntityDto>> (json, jsonOptions)!;
        }

        private Dictionary<string, Item> MergeCatalogs()
        {
            Dictionary<string, Item> all = new Dictionary<string, Item>();
            List<IDictionary> dicts = [this.Generics, this.Keys, this.Equipment];

            foreach (IDictionary dict in dicts)
            {
                foreach (string key in dict.Keys)
                {
                    all.Add(key, (Item)dict[key]!);
                }
            }

            return all;
        }
    }
}
