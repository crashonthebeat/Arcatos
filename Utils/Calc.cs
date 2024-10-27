using Arcatos.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
        northeast,
        up,
        down
    }

    internal static class Calc
    {
        public static Dir Direction(Scene orig, Scene dest)
        {
            (double x1, double y1) = orig.GetRoomCenter();
            (double x2, double y2) = dest.GetRoomCenter();
            
            double rad = Math.Atan2(x2 - x1, y2 - y1);
            rad += Math.PI;
            rad /= Math.PI / 4;

            int slice = Convert.ToInt32(rad) % 8;

            return (Dir)slice;
        }

        // Simple method that returns the opposite direction of a given direction
        public static Dir OppDir (Dir dir)
        { 
            // Cast direction to integer (0:n 1:ne 2:e...)
            // Other side of clock would be adding 4 (n = 0, s = 4)
            // If we go out of bounds, divide by 8 and get remainder to loop back around)
            // Cast this int into the dir enum.

            return (Dir)(((int)dir + 4) % 8); 
        }

        public static (double, double) RoomCenter(Scene scene)
        {
            double x = scene.CornerNW.x + (scene.CornerSE.x / 2);
            double y = scene.CornerNW.y + (scene.CornerSE.y / 2);

            return (x, y);
        }

        // This method will return the distance from a corner that would be considered a diagonal direction.
        public static double SceneCornerWidth(int Side)
        {
            // Using the formula for determining the side lengths (x) of an Octagon, to get the corner (c) distance
            // of the triangular cutout on the side Side = x + 2c, where x = Side / (1 + Math.Sqrt(2))
            return Side / (2 + Math.Sqrt(2));
        }
    }
}
