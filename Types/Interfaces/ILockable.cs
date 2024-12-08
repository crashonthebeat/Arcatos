using Arcatos.Types.Items;

namespace Arcatos.Types.Interfaces
{
    internal interface ILockable
    {
        bool Unlock(Item key);
    }
}
