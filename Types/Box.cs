using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcatos.Types.Interfaces;
using Arcatos.Utils;

namespace Arcatos.Types
{
    public enum BoxType { Ext, Int }
    
    public class Box
    {
        // A dictionary of each Item to its quantity
        public Dictionary<Item, int> Items { get; set; }
        private IEntity _parentEntity;
        private BoxType _boxType;

        public Box(IEntity parent, BoxType type)
        {
            Items = new Dictionary<Item, int>();
            this._parentEntity = parent;
            this._boxType = type;
        }

        public void AddItem(Item item)
        {
            if (Items.ContainsKey(item)) Items[item]++;
            else Items[item] = 1;
        }
        
        public bool RemoveItem(Item item)
        {
            if (Items[item] == 1)
            {
                Items.Remove(item);
                return true;
            }
            else if (Items[item] > 1)
            {
                Items[item]--;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ListItems()
        {
            string p = this._boxType switch
            {
                BoxType.Ext => "on",
                BoxType.Int => "in",
                _ => "around"
            };
            
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            // This is going to get integrated into the narration function.
            if (this._parentEntity.GetType() == typeof(Player))
            {
                Game.Narrate($"You are holding:");
            }
            else
            {
                Game.Narrate($"You see {p} {this._parentEntity.Glance()}:");
            }
            Console.ResetColor();
            foreach (Item item in this.Items.Keys)
            {
                Game.Narrate(item.Glance());
            }
        }

        public List<Item>? FindItem(string name)
        {
            Dev.Log($"Searching {this._parentEntity.ToString()}");
            // Find all objects that match the search name
            List<Item> found = Items.Keys.Where(item => item.Name == name).ToList();

            // If no items found, try again by the summary (in case summary was displayed in inventory)
            if (found.Count > 0) return found;
            else return Items.Keys.Where(item => item.summary.Contains(name)).ToList();
        }
    }
}
