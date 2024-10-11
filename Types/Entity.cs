using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Arcatos.Types.Interfaces;

namespace Arcatos.Types
{
    // Base class for all game objects. 
    public abstract class Entity : IEntity
    {
        public string Name { get; init; }
        internal string id;
        internal string summary;
        internal string desc;
        internal Dictionary<int, Entity> reveal;
        public string EntityType { get; set; }
        public bool IsKnown { get; set; }
        public abstract Box Inventory{ get; set;}

        public Entity(string id, string summary, string[] desc, string name = "$mundane", bool isKnown = false)
        {
            this.EntityType = "base";
            this.id = id;
            // Some items are not going to have a separate name, in this case, parse the summary to have an article.
            if (name == "$mundane")
            {
                this.Name = (new[] {'a','e','i','o','u'}).Contains(summary[0]) ? $"an {summary}" : $"a {summary}";
            }
            else
            {
                this.Name = name;
            }
            this.summary = summary;
            this.desc = String.Concat(desc);
            this.IsKnown = isKnown;
            this.reveal = new Dictionary<int, Entity>();
        }

        // Examine returns a narrative description of the entity as well as additional entities that are only
        // noticed on a specific perception check score. Entities can be an item, exit, clue, or person of interest.
        public virtual void Examine()
        {
            string s = desc;


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

            Game.Narrate([s]);

            if ((this.Inventory != null) && (this.Inventory.Items.Count > 0))
            {
                this.Inventory.ListItems();
            }
            else if ((this.Inventory != null) && (this.Inventory.Items.Count == 0))
            {
                Game.Narrate(["Nothing."]);
            }
        }

        // Glance returns a short description of the item depending on where the item is located and what the item is.
        // This method will change depending on the type of Entity.
        public virtual string Glance()
        {
            // If the first letter of the summary is a vowel, use "an", otherwise use "a".
            string article = ((new[] { 'a', 'e', 'i', 'o', 'u' }).Contains(summary[0])) ? "an" : "a";

            // If the player is familiar with the entity, return its name.
            return (this.IsKnown) ? this.Name : $"{article} {this.summary}";
        }

        public override string ToString()
        {
            return this.Glance();
        }
    }
}
