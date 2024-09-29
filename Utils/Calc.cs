using Arcatos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos.Utils
{
    public enum Dir
    {
        north,
        northwest,
        west,
        southwest,
        south,
        southeast,
        east,
        northeast
    }

    internal static class Calc
    {
        public static Dir Direction(Scene orig, Scene dest)
        {
            double rad = Math.Atan2(dest.x - orig.x, dest.y - orig.y);
            rad += Math.PI;
            rad /= Math.PI / 4;

            int slice = Convert.ToInt32(rad) % 8;

            return (Dir)slice;
        }
    }
}
