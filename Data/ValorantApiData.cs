using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ValorantAnyaBot.Data
{
    public class ValorantApiData
    {
        public static System.Dynamic.ExpandoObject GetSkin(string uuid)
        {
            string url = "https://valorant-api.com/v1/weapons/skinlevels/" +
                            uuid + "?language=ja-JP";
            using (HttpClient w = new HttpClient())
            using (HttpResponseMessage res = w.GetAsync(url).GetAwaiter().GetResult())
            using (HttpContent c = res.Content)
            {
                return JsonSerializer.Deserialize<System.Dynamic.ExpandoObject>(
                    c.ReadAsStringAsync().GetAwaiter().GetResult());
            }
        }
    }
}
