using Arcatos.Types.Interfaces;
using Arcatos.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    
    public class Exit : Entity, ILockable
    {
        public bool IsHidden { get; set; }
        public bool IsClosed { get; set; }  // Whether exit will always appear closed but be traversable.
        public bool IsLocked { get; set; }
        public Dictionary<Scene, Scene> Adjacencies { get; init; }
        public override Box Inventory { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Exit(ExitDto dto, Scene[] scenes) : base(dto.id, dto.summary, dto.desc)
        {
            EntityType = "door";
            
            this.Adjacencies = new Dictionary<Scene, Scene>
            {
                { scenes[0], scenes[1] },
                { scenes[1], scenes[0] }
            };

            this.IsClosed = dto.closed || dto.locked;
            this.IsLocked = dto.locked;
            this.IsHidden = dto.hidden;
        }
        
        // The following four methods manage the exit state, and they are called by the player.
        public bool Open()
        {
            return this.IsClosed && !this.IsLocked;
        }

        public bool Close()
        {
            return !this.IsClosed;
        }

        public bool Lock()
        {
            return this.IsClosed && !this.IsLocked;
        }

        public bool Unlock()
        {
            // Check if Player has key.
            return this.IsLocked;
        }
    }
}
