using Arcatos.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos.Types
{
    public class Boxscope
    {
        // A boxscope is a weird concept that creates a virtual inventory out of all inventories in a specific scope.
        // Ex. The player can have their held inventory, their equipped items, and even equipped inventories.
        // Ex. The scene can have items on the floor, on a table, in a chest, etc. 
        // Yes it would be simpler to just have one inventory for player and scene but I want c o m p l e x i t y.
        // It is also insane to type box as many times as I have in creating this system.
        // To be honest I may not even need most of this as I keep going.

        public List<Box> Player { get; set; }
        public List<Box> Scene { get; set; }
        public List<Box> Local { get; set; }

        public List<Box> GetBoxes(Box box)
        {
            // This is a recursive search of all boxes in a box if the item has a box then the box is searched for more boxes
            List<Box> foundBoxes = new List<Box>();

            foreach (Item item in box.Items.Keys)
            {
                if (item.Inventory != null)
                {
                    foundBoxes.Add(item.Inventory);
                    foundBoxes.AddRange(GetBoxes(item.Inventory));
                }
            }

            return foundBoxes;
        }

        public void UpdateScope(Box box, List<Box> scope, bool isAdded, List<Box>? sourcescope)
        {
            // Scope is made of the held/floor inventory, any inventories in held inventory, and any equipped inventories (recursive)
            List<Box> boxes = GetBoxes(box);

            if (isAdded)
            {
                scope.AddRange(boxes);
                scope.Add(box);

                if (sourcescope != null)
                {
                    sourcescope.Remove(box);
                    foreach (Box subBox in boxes)
                    {
                        scope.Remove(subBox);
                    }
                }
            }
            else
            {
                scope.Remove(box);
                foreach (Box subBox in boxes)
                {
                    scope.Remove(subBox);
                }

                if (sourcescope != null)
                {
                    sourcescope.AddRange(boxes);
                    sourcescope.Add(box);
                }
            }

            UpdateLocalScope();

            // Do you see?
        }

        public void UpdateSceneScope()
        {
            Scene scene = Game.Player.CurrentScene;
            Scene.Clear();

            if (scene.Inventory != null)
            {
                Scene.Add(scene.Inventory);
                List<Box> boxes = GetBoxes(scene.Inventory);
                Scene.AddRange(boxes);
            }

            UpdateLocalScope();
        }

        public void UpdateLocalScope()
        {
            Local = Scene.Concat(Player).ToList();
        }
    }
}
