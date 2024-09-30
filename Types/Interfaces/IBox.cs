using Arcatos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos.Types.Interfaces
{
    public interface IBox
    {
        public Entity RemoveItem();
        public bool AddItem();
    }
}
