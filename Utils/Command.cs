using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Arcatos.Utils
{
    // This is some schoolhouse rock shit.
    public class Command
    {
        public string Action { get; set; }
        public string DirObj { get; set; }
        public string? IndObj { get; set; }
        private Dictionary<string, string> Directions = new Dictionary<string, string>()
        {
            { "n", "north" }, { "s", "south" }, { "w", "west" }, { "e", "east" }, 
            { "nw", "northwest" }, { "ne", "northeast" }, { "sw", "southwest" }, { "se", "southeast" },
            { "u", "up" }, { "d", "down" }
        };


        public Command(string[] input)
        {
            List<string> p = input.Except(["i", "you", "me", "the", "a", "an"]).ToList();

            this.Action = p[0];

            // Check if player just entered a direction or a directional letter.
            if (Directions.ContainsKey(this.Action))
            {
                this.DirObj = Directions[this.Action];
                this.Action = "go";
            }
            else if (Directions.ContainsValue(this.Action))
            {
                this.DirObj = this.Action;
                this.Action = "go";
            }
            else
            {
                this.DirObj = GetObjects(p[1..]);
            }

            Dev.Log($"Action - {this.Action}");
            Dev.Log($"Direct Object - {this.DirObj}");
            Dev.Log($"Indirect Object - {this.IndObj}");
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
            if (this.Directions.ContainsValue(obj))
            {
                return obj;
            }
            else
            {
                this.IndObj = String.Join(" ", input[(i + 1)..]);
            }

            return String.Join(" ", input[0..i]);
        }
    }
}
