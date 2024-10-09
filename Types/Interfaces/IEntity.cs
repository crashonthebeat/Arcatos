using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos.Types.Interfaces
{
    public interface IEntity
    {
        public string ToString();
        public void Examine();   // Get Method for Description
        public string Glance();
    }
}
