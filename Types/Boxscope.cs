using Arcatos.Engine;
using Arcatos.Types.Interfaces;
using Arcatos.Types.Items;

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

        public List<Box> Player { get; private set; } = [ ];
        public List<Box> Scene  { get; private set; } = [ ];
        public List<Box> Local  { get; private set; } = [ ];

        public static bool Debug = false;

        private static List<Box> GetBoxes(Box box)
        {
            // This is a recursive search of all boxes in a box if the item has a box then the box is searched for more boxes
            List<Box> foundBoxes = [];

            foreach (Item item in box.Items.Keys)
            {
                IBox box1 = item as IBox;
                if (box1 == null) continue;
                if (foundBoxes.Contains(box1.Inventory)) continue;
                foundBoxes.Add(box1.Inventory);
                foundBoxes.AddRange(GetBoxes(box1.Inventory));
            }

            foundBoxes.Add(box);
            return foundBoxes;
        }

        private static void UpdatePlayer()
        {
            List<Box> heldBoxes = Boxscope.GetBoxes(Game.Player.HeldItems);
            //List<Box> wornBoxes = GetBoxes(Game.Player.Equipment);
            //Game.Boxscope.Player = heldBoxes.Concat(wornBoxes).ToList();

            Game.Boxscope.Player = heldBoxes;
        }

        private static void UpdateScene()
        {
            Game.Boxscope.Scene = Boxscope.GetBoxes(Game.Player.CurrentScene.Inventory);
        }

        public static void UpdateLocal()
        {
            Boxscope.UpdatePlayer();
            Boxscope.UpdateScene();
            Game.Boxscope.Local = Game.Boxscope.Scene.Concat(Game.Boxscope.Player).ToList();
        }
    }
}
