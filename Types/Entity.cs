using System;
using System.Collections.Generic;
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
        public string EntityType;
        public Box? Inventory = null;

        public Entity(string id, string name, string summary, string[] desc)
        {
            this.EntityType = "base";
            this.id = id;
            this.Name = name;
            this.summary = summary;
            this.desc = String.Concat(desc);
            this.reveal = new Dictionary<int, Entity>();
        }

        // Examine returns a narrative description of the entity as well as additional entities that are only
        // noticed on a specific perception check score. Entities can be an item, exit, clue, or person of interest.
        public void Examine()
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
        }

        // Glance returns a short description of the item depending on where the item is located and what the item is.
        // This method will change depending on the type of Entity.
        public void Glance()
        {
            Game.Narrate([summary]);
        }
    }
}
