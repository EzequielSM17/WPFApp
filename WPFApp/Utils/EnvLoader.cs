

using System.IO;

namespace Utils
{
    public static class EnvLoader
    {
        public static Dictionary<string, string> LoadEnvFile(string path = "config.env")
        {
            var dict = new Dictionary<string, string>();

            if (!File.Exists(path))
                return dict;

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimStart().StartsWith("#")) continue;
                if (!line.Contains("=")) continue;

                var parts = line.Split("=", 2);
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                dict[key] = value;
            }

            return dict;
        }
    }
}
