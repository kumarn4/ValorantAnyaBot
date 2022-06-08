using System.Net.Http;
using System.Text.Json;

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
