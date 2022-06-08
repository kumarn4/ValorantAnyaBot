using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace ValorantAnyaBot.Services
{
    public class ValorantSkinTierService
    {
        private FileInfo _f;
        private ValorantApiSkinsJson skins;
        public ValorantSkinTierService()
        {
            _f = new FileInfo("skins.json");
            if (!_f.Exists)
            {
                string url = "https://valorant-api.com/v1/weapons/skins?language=ja-JP";
                File.Create(_f.FullName).Close();
                using (HttpClient c = new HttpClient())
                using (HttpResponseMessage res = c.GetAsync(url).GetAwaiter().GetResult())
                using (HttpContent con = res.Content)
                {
                    File.WriteAllText(
                        _f.FullName,
                        con.ReadAsStringAsync().GetAwaiter().GetResult());
                }
            }
            skins = JsonSerializer.Deserialize<ValorantApiSkinsJson>(
                _f.OpenText().BaseStream);
        }

        public int GetSkinPrice(Color c)
        {
            if (c.RawValue == 0x009587) return 1275;
            if (c.RawValue == 0xf1b82d) return -2175;
            if (c.RawValue == 0xd1548d) return 1775;
            if (c.RawValue == 0x5a9fe2) return 875;
            if (c.RawValue == 0xefeb65) return 2475;
            return -2;
        }

        public Color GetTierColor(string name)
        {
            var s = skins.data.Find(x => x.displayName == name);
            string url = "https://valorant-api.com/v1/contenttiers/" +
                s.contentTierUuid;
            using (HttpClient c = new HttpClient())
            using (HttpResponseMessage res = c.GetAsync(url).GetAwaiter().GetResult())
            using (HttpContent con = res.Content)
            {
                dynamic d = JsonSerializer.Deserialize<System.Dynamic.ExpandoObject>(
                    con.ReadAsStringAsync().GetAwaiter().GetResult());
                string a = d.data.GetProperty("highlightColor").GetString();
                return new Color(Convert.ToUInt32(a.Replace("33", ""), 16));
            }

        }

        public class ValorantApiSkinsJson
        {
            public int status { get; set; }
            public List<Data> data { get; set; } = new List<Data>();

            public class Data
            {
                public string displayName { get; set; }
                public string contentTierUuid { get; set; }
            }
        }
    }
}
