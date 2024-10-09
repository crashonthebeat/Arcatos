using Arcatos.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public Boxscope()
        {
            this.Player = new List<Box>();
            this.Scene = new List<Box>();
            this.Local = new List<Box>();
        }

        public static List<Box> GetBoxes(Box box)
        {
            // This is a recursive search of all boxes in a box if the item has a box then the box is searched for more boxes
            List<Box> foundBoxes = new List<Box>();

            foreach (Item item in box.Items.Keys)
            {
                if (item.Inventory != null && !foundBoxes.Contains(item.Inventory))
                {
                    foundBoxes.Add(item.Inventory);
                    foundBoxes.AddRange(GetBoxes(item.Inventory));
                }
            }

            foundBoxes.Add(box);
            return foundBoxes;
        }

        public static void UpdatePlayer()
        {
            List<Box> heldBoxes = GetBoxes(Game.Player.HeldItems);
            //List<Box> wornBoxes = GetBoxes(Game.Player.Equipment);
            //Game.Boxscope.Player = heldBoxes.Concat(wornBoxes).ToList();

            Game.Boxscope.Player = heldBoxes;
        }
        
        public static void UpdateScene()
        {
            Game.Boxscope.Scene = GetBoxes(Game.Player.CurrentScene.Inventory);
        }

        public static void UpdateLocal()
        {
            Boxscope.UpdatePlayer();
            Boxscope.UpdateScene();
            Game.Boxscope.Local = Game.Boxscope.Scene.Concat(Game.Boxscope.Player).ToList();
        }
    }
}
