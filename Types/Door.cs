using Arcatos.Types.Interfaces;
using Arcatos.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arcatos.Types
{
    public struct ExitDto
    {
        [JsonInclude] public required string id;
        [JsonInclude] public required string summary;
        [JsonInclude] public required string[] desc;
        [JsonInclude] public required string[] scenes;
        [JsonInclude] public bool closed;
        [JsonInclude] public bool remainsClosed;
        [JsonInclude] public bool locked;
        [JsonInclude] public bool hidden;
    }
    
    public class Door : Entity, ILockable
    {
        public bool IsHidden { get; set; }
        public bool isClosed { get; set; }
        public bool remainsClosed { get; set; }     // Whether door will always appear closed but be traversable.
        public bool isLocked { get; set; }
        public Dictionary<Scene, Scene> Adjacencies { get; init; }
        public override Box Inventory { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Door(ExitDto dto, Scene[] scenes) : base(dto.id, dto.summary, dto.desc)
        {
            EntityType = "door";
            
            this.Adjacencies = new Dictionary<Scene, Scene>();
            this.Adjacencies.Add(scenes[0], scenes[1]);
            this.Adjacencies.Add(scenes[1], scenes[0]);

            this.isClosed = dto.closed || dto.locked || dto.remainsClosed;
            this.remainsClosed = dto.remainsClosed;
            this.isLocked = dto.locked;
            this.IsHidden = dto.hidden;
        }
        
        // The following four methods manage the exit state, and they are called by the player.
        public bool Open()
        {
            return this.isClosed && !this.isLocked;
        }

        public bool Close()
        {
            return !this.isClosed;
        }

        public bool Lock()
        {
            return this.isClosed && !this.isLocked;
        }

        public bool Unlock()
        {
            // Check if Player has key.
            return this.isLocked;
        }
    }
}
