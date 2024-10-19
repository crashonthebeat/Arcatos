using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos.Types.Interfaces
{
    interface ILockable
    {
        bool Unlock();
        bool Lock();
    }
}
