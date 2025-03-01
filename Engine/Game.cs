﻿using System.Text.Json;
using Arcatos.Types;
using Arcatos.Types.Items;
using Arcatos.Utils;

namespace Arcatos.Engine
{
    public class Game
    {        
        public static bool                                             Playing      { get; private set; }
        public static World                                            CurrentWorld { get; set; } = null!;
        public static Player Player { get; private set; } = null!;
        public static ItemCatalog Items { get; private set; } = new ItemCatalog();
        public static Dictionary<string,ItemDto> Templates { get; private set; } = Game.LoadItemTemplates(); // Unique items like containers that should not be duplicated.
        public static Boxscope                                         Boxscope     { get; private set; } = null!;
        // public static Dictionary<string, Dictionary<string, string[]>> Narration;
        // public static Random                                           Random = new Random();
        
        // TODO: Make this a static constructor? Not sure how those work yet
        public Game(string currentSceneId)
        {
            // Get WorldId and Scene for Player State
            string worldId = currentSceneId.Split('_')[0];
            Game.CurrentWorld = new World(worldId);
            (_, Scene currentScene) = Game.CurrentWorld.GetMapObjects(currentSceneId);

            // Load Player (should actually load from file either a saved state or new player state generated by char creation)
            Game.Player = new Player(currentScene);

            // HAIL BOXSCOPE
            Game.Boxscope = new Boxscope();
            Boxscope.UpdateLocal();
            
            // and now u can play
            Game.Playing = true;
        }

        public static void Play()
        {
            Game.Player.CurrentScene.Enter();

            Game.Prompt();
        }

        // This is the player interface.
        // TODO: This whole interface section will be redone when the game gets more complex.
        // tbh this whole class will probably and definitely change.
        public static void Prompt()
        {
            while (Game.Playing)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (String.IsNullOrEmpty(input)) 
                {
                    Game.Write("You didn't enter a command.");
                }
                else 
                {
                    // Create command object from player input.
                    Command command = new Command(input.ToLower().Split(' '));

                    Game.Playing = Game.Player.Execute(command);
                }
            }
        }

        // LoadUniqueItems all the files from the unique folder and loads them as item models for the map processing to instantiate as new objects
        // This allows for less duplication of entries in the unique items list for things like containers that have common properties.
        private static Dictionary<string,ItemDto> LoadItemTemplates()
        {
            // Grab all the item files
            string[] templateFiles = Directory.GetFiles(Path.Combine(Program.Dir, "Data", "World", "Items", "templates"));
            
            // Create new dictionary to return to unique items.
            // Key difference between this and common items is that this dict is string to *dto*, not the item itself.
            Dictionary<string,ItemDto> models = new Dictionary<string, ItemDto>();

            foreach (string file in templateFiles)
            {
                using FileStream json = File.OpenRead(file);
                foreach (KeyValuePair<string, ItemDto> item in JsonSerializer.Deserialize<Dictionary<string, ItemDto>>(json)!)
                {
                    models.Add(item.Key, item.Value);
                }
            }

            return models;
        }

#region Narration
        // Write is a wrapper on top of console.write that takes game narration and presents it to the player.
        // This will have hella overloads, and in fact be broken out into several methods.
        public static void Write(string s)
        {
            Console.WriteLine(s);
        }

        // Narrate overload for a list of strings perchance fed by an Entity's examine method that adds extra topics.
        public static void Write(string[] raw)
        {
            string joined = "";
            
            foreach (string s in raw)
            {
                joined += $"{s.Trim()} ";
            }

            Game.Write(joined);

            // Todo: Wrap strings based on a detected (or set or standard) console.
        }

        // Makes strings into a proper title case for nicer output
        public static string Titleize(string s)
        {
            string[] ignoredWords = ["the", "a", "an", "to", "from", "by", "of"];
            
            List<string> words = s.Split(' ').ToList();
            string   output = "";

            // If the first word is a or an, that's the only time it should get capitalized.
            switch (words[0])
            {
                case "a":
                    output += "A ";
                    words.RemoveAt(0);
                    break;
                case "an":
                    output += "An ";
                    words.RemoveAt(0);
                    break;
            }
            
            // Loop through the rest of the words
            foreach (string word in words)
            {
                // Exceptions for capitalization
                if (ignoredWords.Contains(word))
                {
                    output += $"{word} ";
                    continue;
                }
                
                // Otherwise, capitalize the first letter of each word.
                output += word.Length == 1 ? $"{word.ToUpper()} " : $"{Char.ToUpper(word[0])}{word[1..]} ";
            }
            
            // Return the output and remove the last space.
            return output.Trim();
        }
        
    }

#endregion
}
