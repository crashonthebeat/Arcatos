using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos.Types.Interfaces
{
    interface IEntity
    {
        public string Examine(int check);   // Get Method for Description
        public string Glance();             // Get Method for Summary
    }
}
