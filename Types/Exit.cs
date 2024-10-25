﻿using Arcatos.Types.Interfaces;
using Arcatos.Utils;
using System.Text.Json.Serialization;

namespace Arcatos.Types
{
    public struct ExitDto
    {
        [JsonInclude] public required string id;
        [JsonInclude] public required string summary;
        [JsonInclude] public required string[] desc;
        [JsonInclude] public required string[] scenes;
        [JsonInclude] public bool closed;
        [JsonInclude] public bool locked;
        [JsonInclude] public bool hidden;
    }
    
    public class Exit : Entity, ILockable
    {
        public bool IsClosed { get; }  // Whether exit always appears closed (will never show next room on look)
        public bool IsLocked { get; set; }
        public bool IsHidden { get; set; }
        public Dictionary<Scene, Scene> AdjScenes { get; }
        public (double x, double y) Loc { get; }
        public override Box Inventory { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Exit(ExitDto dto, Scene[] scenes) : base(dto.id, dto.summary, dto.desc)
        {
            this.AdjScenes = new Dictionary<Scene, Scene>
            {
                { scenes[0], scenes[1] },
                { scenes[1], scenes[0] }
            };

            this.IsClosed = dto.closed;
            this.IsLocked = dto.locked;
            this.IsHidden = dto.hidden;
            this.Loc = GetPosition(scenes[0], scenes[1]);
        }
        
        // The following four methods manage the exit state, and they are called by the player.
        public bool Lock()
        {
            return !this.IsLocked;
        }

        public bool Unlock()
        {
            // Check if Player has key.
            return this.IsLocked;
        }

        // GetPosition is called at construction to return the coordinates of the exit on the map.
        private static (double, double) GetPosition(Scene orig, Scene dest)
        {
            // First we check which walls are intersecting, for that we need to get the wall positions of each scene.
            (double n, double e, double s, double w) origWalls = orig.GetWallPositions();
            (double n, double e, double s, double w) destWalls = dest.GetWallPositions();

            // Find which values match
            (bool n, bool e, bool s, bool w) = (Math.Abs(origWalls.n - destWalls.s) <= 0, 
                                                Math.Abs(origWalls.e - destWalls.w) <= 0,
                                                Math.Abs(origWalls.s - destWalls.n) <= 0, 
                                                Math.Abs(origWalls.w - destWalls.e) <= 0);


            double wallPos;
            // Now make a decision based on the matches. If two walls match, say n and e, that means the meet point is
            // on a corner, so the only possible resolution is to return the coordinates of that corner.
            switch (n, e, s, w)
            {
                // Northeast Corner Match
                case (true, true, false, false):
                    return (origWalls.e, origWalls.n);
                // Southeast Corner Match
                case (false, true, true, false):
                    return (origWalls.e, origWalls.s);
                // Northwest Corner Match
                case (true, false, false, true):
                    return (origWalls.w, origWalls.n);
                // Southwest Corner Match
                case (false, false, true, true):
                    return (origWalls.w, origWalls.s);
                // North Wall Match
                case (true, false, false, false):
                    wallPos = origWalls.n;
                    break;
                // East Wall Match
                case (false, true, false, false):
                    wallPos = origWalls.e;
                    break;
                // South Wall Match
                case (false, false, true, false):
                    wallPos = origWalls.s;
                    break;
                // West Wall Match
                case (false, false, false, true):
                    wallPos = origWalls.w;
                    break;
                default:
                    throw new WeastException("Could not find matching wall");
            }

            // Next need to find the points where the walls meet.

            double min, max;   // The bounds of the wall's meeting point
            if (n ^ s)      // If only north or only south.
            {
                // If it's a north/south match, find the min of the east points and the max of the west points
                min = Math.Max(orig.CornerNW.x, dest.CornerNW.x);
                max = Math.Min(orig.CornerSE.x, dest.CornerSE.x);

                // Inline position is the x coordinate, Wall position is the y coordinate.
                return ((min + max) / 2, wallPos);
            }
            if (e ^ w) // If only east or only west (XOR).
            {
                // If it's an east/west match, find the min of the south points and the max of the north points
                min = Math.Max(orig.CornerNW.y, dest.CornerNW.y);
                max = Math.Min(orig.CornerSE.y, dest.CornerSE.y);

                // Wall Position will be the x coordinate, the Inline position the y coordinate.
                return (wallPos, (min + max) / 2);
            }
            
            throw new WeastException("Could not place exit along wall");
        }
    }
}
