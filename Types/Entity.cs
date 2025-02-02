using Arcatos.Types.Interfaces;
using System.Text.Json.Serialization;


namespace Arcatos.Types
{
    public struct EntityDto
    {
        [JsonInclude] public          string?                  Name;
        [JsonInclude] public required string                   Summary;
        [JsonInclude] public required string[]                 Description;
        [JsonInclude] public          bool?                    IsConsumable;
        [JsonInclude] public          bool?                    Visited;
        [JsonInclude] public          int                      Layer;
        [JsonInclude] public          Dictionary<string, int>? Slots;
    }
    
    // Base class for all game objects. 
    public abstract class Entity : IEntity
    {
        public   string                  Name { get; init; }
        internal string                  id;
        public   string                  Id { get => this.id; }
        internal string                  summary;
        internal string                  desc;
        internal Dictionary<int, Entity> Reveal;
        public   bool                    IsKnown { get; private set; }
        // public abstract Box Inventory{ get; set; }
        
        public static bool Debug = false;

        protected Entity(string id, string summary, string[] desc, string name = "$mundane", bool isKnown = false)
        {
            this.id = id;
            // Some items are not going to have a separate name, in this case, parse the summary to have an article.
            this.Name = name != "$mundane" ? name
                : Game.Titleize(new[] {'a','e','i','o','u'}.Contains(summary[0]) ? $"an {summary}" : $"a {summary}");

            this.summary = summary;
            this.desc = String.Concat(desc);
            this.IsKnown = isKnown;
            this.Reveal = new Dictionary<int, Entity>();
        }

        // Examine returns a narrative description of the entity as well as additional entities that are only
        // noticed on a specific perception check score. Entities can be an item, exit, clue, or person of interest.
        public virtual void Examine()
        {
            string s = this.desc;


            // TODO:
            // Add extra narration strings for a perception check
            // And conditional strings depending on the character, environment, etc.
            //foreach (KeyValuePair<int, Entity> kvp in reveal)
            //{
            //    if (check >= kvp.Key)
            //    {
            //        s += $" {kvp.Value.Glance()}";
            //    }
            //}

            Game.Write(s);
            
            // switch (this.Inventory.Items.Count)
            // {
            //     case > 0:
            //         this.Inventory.ListItems();
            //         break;
            //     case 0:
            //         Game.Narrate("Nothing.");
            //         break;
            // }
        }

        // Glance returns a short description of the item depending on where the item is located and what the item is.
        // This method will change depending on the type of Entity.
        public virtual string Glance()
        {
            char[] vowels = ['a', 'e', 'i', 'o', 'u'];
            
            // If the first letter of the summary is a vowel, use "an", otherwise use "a".
            string article = vowels.Contains(this.summary[0]) ? "an" : "a";

            // If the player is familiar with the entity, return its name.
            return this.IsKnown ? this.Name : $"{article} {this.summary}";
        }

        public virtual bool Learn()
        {
            // This method will be overridden with more complex types.
            this.IsKnown = true;
            return this.IsKnown;
        }

        public override string ToString()
        {
            return this.Glance();
        }
    }
}
