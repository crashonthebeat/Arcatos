namespace Arcatos.Utils
{
    // This is some schoolhouse rock shit.
    public class Command
    {
        public string  Action { get; }
        public string  DirObj { get; }
        public string? IndObj { get; set; }
        private readonly Dictionary<string, string> _directions = new()
        {
            { "n", "north" }, { "s", "south" }, { "w", "west" }, { "e", "east" }, 
            { "nw", "northwest" }, { "ne", "northeast" }, { "sw", "southwest" }, { "se", "southeast" },
            { "u", "up" }, { "d", "down" }
        };

        private const bool Debug = false;


        public Command(string[] input)
        {
            List<string> p = input.Except(["i", "you", "me", "the", "a", "an"]).ToList();

            this.Action = p[0];

            // Check if player just entered a direction or a directional letter.
            if (this._directions.TryGetValue(this.Action, out string? value))
            {
                this.DirObj = value;
                this.Action = "go";
            }
            else if (this._directions.ContainsValue(this.Action))
            {
                this.DirObj = this.Action;
                this.Action = "go";
            }
            else
            {
                this.DirObj = this.GetObjects(p[1..]);
            }

            Dev.Log($"Action - {this.Action}", Command.Debug);
            Dev.Log($"Direct Object - {this.DirObj}", Command.Debug);
            Dev.Log($"Indirect Object - {this.IndObj}", Command.Debug);
        }

        private string GetObjects(List<string> input)
        {
            string[] prepositions = ["at", "by", "from", "in", "on", "to", "with", "into"];
            
            string? foundPrep = input.Intersect(prepositions).FirstOrDefault();
            
            // If there's no preposition, there's no indirect object. Return only direct object.
            if (String.IsNullOrEmpty(foundPrep))
            {
                return String.Join(" ", input);
            }

            // Grab Index of preposition and then take all after it as the indirect object.
            int i = input.IndexOf(foundPrep);
            string obj = String.Join(" ", input[(i + 1)..]);

            // Check if player put direction after preposition like "walk to the north"
            if (this._directions.ContainsValue(obj))
            {
                return obj;
            }

            this.IndObj = String.Join(" ", input[(i + 1)..]);

            return String.Join(" ", input[..i]);
        }
    }
}
