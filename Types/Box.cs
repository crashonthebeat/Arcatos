using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos.Types
{
    public class Box
    {
        // A dictionary of each Item to its quantity
        public Dictionary<Item, int> Items { get; set; }

        public Box()
        {
            Items = new Dictionary<Item, int>();
        }

        public List<Item>? FindItem(string name)
        {
            // Find all objects that match the search name
            List<Item> found = Items.Keys.Where(item => item.Name == name).ToList();

            if (found.Count > 0)
            {
                return found;
            }
            else
            {
                // If no items found, try again by the summary (in case summary was displayed in inventory)
                return Items.Keys.Where(item => item.summary.Contains(name)).ToList();
            }
        }
    }
}
