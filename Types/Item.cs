using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos.Types
{
    public class Item : Entity
    {
        public Item (string id, string name, string summary, string[] desc) : base(id, name, summary, desc)
        {

        }
    }
}
