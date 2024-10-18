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
        northeast
    }

    internal static class Calc
    {
        public static Dir Direction(Scene orig, Scene dest)
        {
            (double x, double y) origCenter = RoomCenter(orig);
            (double x, double y) destCenter = RoomCenter(dest);
            
            double rad = Math.Atan2(destCenter.x - origCenter.x, destCenter.y - origCenter.y);
            rad += Math.PI;
            rad /= Math.PI / 4;

            int slice = Convert.ToInt32(rad) % 8;

            return (Dir)slice;
        }

        

        // Calc.RoomDirection takes the scene calling the method and the scene that will be the destination, and returns a list of possible
        // directions for the destination scene. 
        public static List<Dir> RoomDirection (Scene orig, Scene dest)
        {            
            // The method needs to find which wall would be the intersecting wall (n, s, e, w)
            // So it will check if any of the orig scenes's walls match the outside of the dest scene's walls.

            bool n = (orig.loc.y == dest.loc.y + dest.size.y + 1);
            bool s = (orig.loc.y + orig.size.y == dest.loc.y - 1);
            bool e = (orig.loc.x == dest.loc.x + dest.size.x + 1);
            bool w = (orig.loc.x + orig.size.x == dest.loc.x - 1);

            // Catch all corner adjacencies, in these cases only one direction is valid.
            Dir dir = Dir.north;
            switch (n, s, e, w) 
            {
                // North and East walls match
                case (true, false, true, false):
                    return [Dir.northeast];
                // North and West walls match
                case (true, false, false, true):
                    return [Dir.northwest];
                // South and East walls match
                case (false, true, true, false):
                    return [Dir.southeast];
                // South and West walls match
                case (false, true, false, true):
                    return [Dir.southwest];
                case (true, false, false, false):
                    dir = Dir.north;
                    break;
                case (false, true, false, false):
                    dir = Dir.south;
                    break;
                case (false, false, true, false):
                    dir = Dir.east;
                    break;
                case (false, false, false, true):
                    dir = Dir.north;
                    break;
            }

            // Now we need the intersecting scenes 
            // Get the intersecting cells of both scenes on the wall

            Dir destDir = (Dir)(((int)dir + 4) % 8);

            (int origEdge, List<int> origCells) = Calc.EdgeCells(orig, dir);
            (int destEdge, List<int> destCells) = Calc.EdgeCells(dest, destDir);
            List<int> intersect = origCells.Intersect(destCells).ToList();
            double exitLoc = intersect.Average();

            // Now we have all the information we need to start building possible directions.
            // HERE BE COMPLEX BOOLEANS
            List<Dir> possibleDirections = [];
            (double x, double y) origCenter = RoomCenter(orig);

            // Find priority
            // If the exit Location is at scene center, then the wall's direction returned by default.
            if (exitLoc == origCenter.x || exitLoc == origCenter.y)
            {
                return [ dir ];
            }
            // Take the distance of the wall LEN divided by 4 and set it to QUARTERLEN
            // If the exit location is less than the center plus/minus a QUARTERLEN, then the wall direction should be added to the list.
            else if (((orig.size.x / 4) + origCenter.x >= exitLoc && exitLoc >= (orig.size.x / 4) - origCenter.x && !e && !w) ||
                     ((orig.size.y / 4) + origCenter.y >= exitLoc && exitLoc >= (orig.size.y / 4) - origCenter.y && !n && !s))
            {
                possibleDirections.Add(dir);
            }
            
            if (exitLoc > origCenter.x && !e && !w && n) possibleDirections.Add(Dir.northwest);
            else if (exitLoc > origCenter.x && !e && !w && s) possibleDirections.Add(Dir.southwest);
            else if (exitLoc < origCenter.x && !e && !w && n) possibleDirections.Add(Dir.northeast);
            else if (exitLoc < origCenter.x && !e && !w && s) possibleDirections.Add(Dir.southeast);
            else if (exitLoc > origCenter.y && !n && !s && w) possibleDirections.Add(Dir.southwest);
            else if (exitLoc > origCenter.y && !n && !s && e) possibleDirections.Add(Dir.southeast);
            else if (exitLoc < origCenter.y && !n && !s && w) possibleDirections.Add(Dir.northwest);
            else if (exitLoc < origCenter.y && !n && !s && e) possibleDirections.Add(Dir.northeast);

            // If the intersect range contains scene center, add it to the list.
            if ((intersect[0] <= origCenter.x && origCenter.x <= intersect[-1] && !e && !w) || 
                (intersect[0] <= origCenter.y && origCenter.y <= intersect[-1] && !n && !s))
            {
                possibleDirections.Add(dir);
            }
            
            return possibleDirections;

        }

        public static (double, double) RoomCenter(Scene scene)
        {
            double x = scene.loc.x + (scene.size.x / 2);
            double y = scene.loc.y + (scene.size.y / 2);

            return (x, y);
        }

        // Calc.ExitLoc returns the intersecting cells and the average of those cells, which is the x or y value of the exit along the wall.
        public static (double, List<int>) ExitLoc (Scene orig, Scene dest, Dir dir)
        {
            Dir destDir = (Dir)(((int)dir + 4) % 8);

            (int origEdge, List<int> origCells) = Calc.EdgeCells(orig, dir);
            (int destEdge, List<int> destCells) = Calc.EdgeCells(dest, destDir);
            List<int> intersect = origCells.Intersect(destCells).ToList();

            return (intersect.Average(), intersect);
        }


        // Calc.EdgeCells takes a scene and a direction and returns all the cells along that wall. The first int is either the x or y location of the wall
        // The array is all the x or y values along that wall.
        public static (int, List<int>) EdgeCells(Scene scene, Dir dir)
        {
            // Get cells on wall
            int i;
            int[] j;
            switch (dir)
            {
                case Dir.north:
                    i = scene.loc.y;
                    j = Calc.RangeArray(scene.loc.x, scene.size.x);
                    break;
                case Dir.south:
                    i = scene.loc.y + scene.size.y;
                    j = Calc.RangeArray(scene.loc.x, scene.size.x);
                    break;
                case Dir.east:
                    i = scene.loc.x;
                    j = Calc.RangeArray(scene.loc.y, scene.size.y);
                    break;
                case Dir.west:
                    i  = scene.loc.x + scene.size.x;
                    j = Calc.RangeArray(scene.loc.y, scene.size.y);
                    break;
                default:
                    throw new Exception(); // TODO: Make new exception for this.
            }

            return (i, j.ToList());
        }

        // Quick method for returning the cells in the range of the start and end.
        public static int[] RangeArray(int start, int len)
        {
            int[] range = new int[len];

            for (int i = 0; i < len; i++)
            {
                range[i] = start + i;
            }

            return range;
        }
    }
}
