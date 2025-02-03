using System.Text.RegularExpressions;

namespace Arcatos.Utils
{
    public static class Narrator
    {
        private static Dictionary<string, string[]> _player;
        private static Dictionary<string, string[]> _scene;
        private static Dictionary<string, string[]> _exit;
        
        private static Random _random = new Random();
        
        public static string Player(string key, string entity = "") => GetPrompt(_player[key], entity);
        public static string Scene(string key, string entity = "") => GetPrompt(_scene[key], entity);
        public static string Exit(string  key, string entity = "") => GetPrompt(_exit[key], entity);

        static Narrator()
        {
            _player = JsonDataMgr.LoadToType<Dictionary<string,string[]>>("Data/Narration/Player.json");
            _scene  = JsonDataMgr.LoadToType<Dictionary<string,string[]>>("Data/Narration/Scene.json");
            _exit  = JsonDataMgr.LoadToType<Dictionary<string,string[]>>("Data/Narration/Exit.json");
        }

        private static string GetPrompt(string[] prompts, string s)
        {
            string       output  = prompts[_random.Next(0, prompts.Length)];
            const string pattern = @"\$([\w]+)\$";
            return Regex.Replace(output, pattern, s);
        }
    }
}
