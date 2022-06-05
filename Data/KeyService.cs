using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ValorantAnyaBot.Data
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
