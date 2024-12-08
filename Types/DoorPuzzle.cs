using System.Text.Json.Serialization;

namespace Arcatos.Types
{
    public class DoorPuzzleDto
    {
        [JsonInclude] public Exit  attachedExit;
        [JsonInclude] public int[] solution;
        [JsonInclude] public int[] minMax;
    }
    
    // Door Puzzle represents anything like a keypad, vault combo lock, etc.
    public class DoorPuzzle
    {
        private readonly Exit  attachedExit;
        private readonly int[] solution;
        private readonly int[] solMinMax; // The min and max value of each solution entry.
        
        public           int[] SolveAttempt { get; set; }

        public DoorPuzzle(DoorPuzzleDto dto)
        {
            this.attachedExit = dto.attachedExit;
            this.solution     = dto.solution;
            this.solMinMax    = dto.minMax;

            this.SolveAttempt = new int[dto.solution.Length];
        }

        public bool CheckSolution()
        {
            if (this.SolveAttempt == this.solution)
            {
                //this.attachedExit.Unlock();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
