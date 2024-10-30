namespace Arcatos.Types.Interfaces
{
    // Somehow I want to separate all of the Box Properties (Items, Add, Remove, etc.) into a separate class/interface so that only certain types inherit
    public interface IBox
    {
        public Box  Inventory{ get; }
        public void ListItems();
    }
}
