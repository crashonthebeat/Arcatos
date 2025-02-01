using System.Text.RegularExpressions;

namespace Arcatos.Utils
{
    public class Narrator
    {
        public  static dynamic Player { get; }
        public  static dynamic Scene  { get; }
        private static Random  _random = new Random();

        static Narrator()
        {
            Player = JsonDataMgr.LoadGeneric("Narration/Player.json");
            Scene  = JsonDataMgr.LoadGeneric("Narration/Scene.json");
        }

        public static void Write(string[] prompts, string s = "")
        {
            string       output  = prompts[_random.Next(0, prompts.Length)];
            const string pattern = @"\$([\w]+)\$";
            Game.Write(Regex.Replace(output, pattern, s));
        }
    }
}
