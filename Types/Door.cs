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
    public class Door : Entity, ILockable
    {
        public bool IsHidden { get; set; }
        private bool isClosed;
        private bool isLocked;
        public Dictionary<Scene, Scene> Adjacencies { get; init; }

        public Door(string id, string summary, string[] desc, Scene[] scenes, bool isClosed = false,
                    bool isLocked = false, bool isHidden = false, string name = "door")
            : base(id, name, summary, desc)
        {
            EntityType = "door";
            
            this.Adjacencies = new Dictionary<Scene, Scene>();
            this.Adjacencies.Add(scenes[0], scenes[1]);
            this.Adjacencies.Add(scenes[1], scenes[0]);

            this.isClosed = isClosed || isLocked;
            this.isLocked = isLocked;
            this.IsHidden = isHidden;
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
