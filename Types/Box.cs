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
        public string Id { get; }
        public Dictionary<Item, int> Items { get; set; }
        private IEntity _parentEntity;
        private BoxType _boxType;

        public Box(IEntity parent, BoxType type)
        {
            Items = new Dictionary<Item, int>();
            this._parentEntity = parent;
            this._boxType = type;
            this.Id = parent.Id;
        }

        public void AddItem(Item item)
        {
            if (!this.Items.TryAdd(item, 1))
            {
                this.Items[item]++;
                Dev.Log($"Increased qty of {item.ToString()} to {this.Items[item]}");
            }
            else
            {
                Dev.Log($"Added new {item.ToString()}");
            }
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

            if (this.Items.Count == 0) return;
            
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            // This is going to get integrated into the narration function.
            Game.Narrate(this._parentEntity.GetType() == typeof(Player) ? $"You are holding:" : $"You see {p} {this._parentEntity.Glance()}:");
            Console.ResetColor();
            foreach (Item item in this.Items.Keys)
            {
                Game.Narrate($"{item.Glance()} x{this.Items[item]}");
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
