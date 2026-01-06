using System.Text.Json;

namespace SenetServer.Shared
{
    public static class UsernameGenerator
    {
        public static string GetNewUsername()
        {
            string adjective = "", noun = "";
            int number = 0;
            string filePath = Path.Combine(AppContext.BaseDirectory, "Shared", "UsernameComponents.json");
            
            using (StreamReader r = new StreamReader(filePath))
            {
                try
                {
                    string json = r.ReadToEnd();
                    UsernameComponents components = JsonSerializer.Deserialize<UsernameComponents>(json);
                    Random random = new Random();
                    adjective = components.Adjectives[random.Next(components.Adjectives.Count)];
                    noun = components.Nouns[random.Next(components.Nouns.Count)];
                    number = random.Next(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return $"{adjective}{noun}{number}";
            }
        }
    }

    public class UsernameComponents
    {
        public List<string> Adjectives { get; set; }
        public List<string> Nouns { get; set; }
    }
}
