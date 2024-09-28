using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcatos.Types.Interfaces;

namespace Arcatos.Types
{
    // Base class for all game objects. 
    abstract class Entity : IEntity
    {
        public string Name { get; set; }
        internal string _id;
        internal string _summary;
        internal string _desc;
        internal Dictionary<int, Entity> _reveal;
        public string EntityType;
        public IBox? Inventory = null;

        public Entity(string id, string name, string summary, string desc)
        {
            EntityType = "base";
            _id = id;
            Name = name;
            _summary = summary;
            _desc = desc;
            _reveal = new Dictionary<int, Entity>();
        }

        // Examine returns a narrative description of the entity as well as additional entities that are only
        // noticed on a specific perception check score. Entities can be an item, exit, clue, or person of interest.
        public string Examine(int check)
        {
            string s = _desc;

            foreach (KeyValuePair<int, Entity> kvp in _reveal)
            {
                if (check >= kvp.Key)
                {
                    s += $" {kvp.Value.Glance()}";
                }
            }

            return s;
        }

        // Glance returns a short description of the item depending on where the item is located and what the item is.
        // This method will change depending on the type of Entity.
        public string Glance()
        {
            return _summary;
        }
    }
}
