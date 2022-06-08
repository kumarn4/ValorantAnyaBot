using System.IO;
using System.Text.Json;

namespace ValorantAnyaBot.Services
{
    public class KeyService
    {
        public static string GetKey()
        {
            FileInfo keyfile = new FileInfo("key.json");
            if (!keyfile.Exists)
            {
                File.Create(keyfile.FullName);
                return null;
            }

            KeyJson key = JsonSerializer.Deserialize<KeyJson>(
                keyfile.OpenText().BaseStream);

            if (string.IsNullOrEmpty(key.Key)) return null;
            return key.Key;
        }

        public class KeyJson
        {
            public string Key { get; set; }
        }
    }
}
