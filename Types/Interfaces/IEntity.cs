namespace Arcatos.Types.Interfaces
{
    public interface IEntity
    {
        public string Id { get; }
        public string ToString();
        public void Examine();   // Get Method for Description
        public string Glance();
    }
}
