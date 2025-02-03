using Arcatos.Engine;
using Arcatos.Types.Interfaces;
using Arcatos.Types.Items;
using Arcatos.Utils;

namespace Arcatos.Types
{
    public enum BoxType { Ext, Int }
    
    public class Box(IEntity parent, BoxType type)
    {
        // A dictionary of each Item to its quantity
        public           string                Id    { get; } = parent.Id;
        public           Dictionary<Item, int> Items { get; } = new();

        private const bool Debug = false;

        public void AddItem(Item item)
        {
            if (!this.Items.TryAdd(item, 1))
            {
                this.Items[item]++;
                Dev.Log($"Increased qty of {item} to {this.Items[item]}", Box.Debug);
            }
            else
            {
                Dev.Log($"Added new {item}", Box.Debug);
            }
        }
        
        public void RemoveItem(Item item)
        {
            switch (this.Items[item])
            {
                case 1:
                    this.Items.Remove(item);
                    return;
                case > 1:
                    this.Items[item]--;
                    return;
                default:
                    return;
            }
        }

        public void ListItems()
        {
            string p = type switch
            {
                BoxType.Ext => "on",
                BoxType.Int => "in",
                _ => "around"
            };

            if (this.Items.Count == 0) return;
            
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            // This is going to get integrated into the narration function.
            Game.Write(parent.GetType() == typeof(Player) 
                             ? "You are holding:" : $"You see {p} {parent.Glance()}:");
            Console.ResetColor();
            foreach (Item item in this.Items.Keys)
            {
                Game.Write($"{item.Glance()} x{this.Items[item]}");
            }
        }

        public List<Item> FindItem(string name)
        {
            Dev.Log($"Searching {parent.ToString()}", Box.Debug);
            // Find all objects that match the search name
            List<Item> found = this.Items.Keys.Where(item => item.Name == name).ToList();

            // If no items found, try again by the summary (in case summary was displayed in inventory)
            return found.Count > 0 ? found : this.Items.Keys.Where(item => item.summary.Contains(name)).ToList();
        }
    }
}
