using Arcatos.Types.Interfaces;
using Arcatos.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos.Types
{
    enum Ex
    {
        Open,
        Closed,
        Locked
    }

    internal class Door : Entity, ILockable
    {
        private readonly Dictionary<Scene, Scene> _adjacencies;
        private bool _isClosed;
        private bool _isLocked;
        public bool IsHidden;

        public Door(string id, string summary, string desc, Scene[] adjacencies, string name = "door",
                    bool isClosed = false, bool isLocked = false, bool isHidden = false)
            : base(id, name, summary, desc)
        {
            EntityType = "door";
            _adjacencies = new Dictionary<Scene, Scene>();
            _adjacencies.Add(adjacencies[0], adjacencies[1]);
            _adjacencies.Add(adjacencies[1], adjacencies[0]);

            _isClosed = isClosed || isLocked;
            _isLocked = isLocked;
            IsHidden = isHidden;
        }

        public Scene GetAdjacent(Scene scene)
        {
            return _adjacencies[scene];
        }

        public bool Open()
        {
            return _isClosed && !_isLocked;
        }

        public bool Close()
        {
            return !_isClosed;
        }

        public bool Lock()
        {
            return _isClosed && !_isLocked;
        }

        public bool Unlock()
        {
            // Check if Player has key.
            return _isLocked;
        }
    }
}
