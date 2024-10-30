namespace Arcatos.Utils
{
    // Dev class holds all the methods used for debugging and logs
    // It also holds "cheat" methods that are "for testing purposes only".
    public static class Dev
    {
        // The purpose of this method is because I find myself constantly doing Console writes when debugging
        // and then just deleting them or commenting them out when I finish, only to have to rewrite or uncomment
        // them when something breaks. This is just a way to keep those messages in with a quick way to include
        // or exclude them from the console output. 
        public static void Log(string msg)
        {
            if (!Program.Settings.DebugMode) return;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"DEBUG: {msg}");
            Console.ResetColor();
        }

        public static void Log(string msg, bool mode)
        {
            if (!mode) return;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"DEBUG: {msg}");
            Console.ResetColor();
        }
    }
}
