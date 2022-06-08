using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using ValNet;
using ValNet.Objects.Authentication;
using ValorantAnyaBot.Data;
using static ValNet.Objects.Store.NightMarket;

namespace ValorantAnyaBot.Services
{
    public class ValorantOfferService
    {
        public static async Task ShowOfferEmbeds(ulong id)
        {
            UserService.UserData u = Program.User.Get(id);
            IDMChannel c = await (await Program.Client.GetUserAsync(id)).CreateDMChannelAsync();

            if (u == null)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $"{c.Name} は登録されていません\n" +
                        $"**`!r [Username] [Password]`**")
                    .WithColor(Color.Red);
                await c.SendMessageAsync(embed: ebb.Build());
                return;
            }

            RiotLoginData logindata = new RiotLoginData()
            {
                username = Program.User.GetDecryptedUsername(u.encrypted_username),
                password = Program.User.GetDecryptedPassword(u.encrypted_password),
            };
            RiotUser user = new RiotUser(logindata);
            var res = await user.Authentication.AuthenticateWithCloud();

            if (!res.bIsAuthComplete)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $" ユーザーネーム : {logindata.username} , " +
                        $"パスワード : {logindata.password} のログインに失敗しました。")
                    .WithColor(Color.Red);
                await c.SendMessageAsync(embed: ebb.Build());
                return;
            }

            List<Embed> es = new List<Embed>();
            await user.Store.GetPlayerStore();
            List<string> offers = user.Store.PlayerStore.SkinsPanelLayout.SingleItemOffers;

            foreach (string item in offers)
            {
                dynamic v = ValorantApiData.GetSkin(item);
                EmbedBuilder eb = new EmbedBuilder()
                    .WithTitle(
                        ((string)v.data.GetProperty("displayName").GetRawText())
                            .Replace("\"", ""));

                if (!string.IsNullOrEmpty(v.data.GetProperty("streamedVideo").GetString()))
                {
                    eb.WithDescription(
                        $"[動画]({v.data.GetProperty("streamedVideo").GetString()})");
                }
                eb.WithImageUrl(v.data.GetProperty("displayIcon").GetString());
                Color color = Program.Tier.GetTierColor(v.data.GetProperty("displayName").GetString());
                eb.WithColor(color);
                int cost = Program.Tier.GetSkinPrice(color);
                if (cost > 0)
                {
                    eb.Title += $" > {cost} VP";
                }
                else
                {
                    eb.Title += $" > {-cost} ～ VP";
                }
                es.Add(eb.Build());
            }

            if (user.Store.PlayerStore.BonusStore != null)
            {
                EmbedBuilder eb = new EmbedBuilder()
                    .WithTitle("ナイトマーケット開催中")
                    .WithColor(Color.Purple)
                    .WithDescription("**`!n`** コマンドでナイトマーケットを確認できます");
                es.Add(eb.Build());
            }

            await c.SendMessageAsync(embeds: es.ToArray());
            return;
        }
        public static async Task ShowNightMarketOfferEmbeds(ulong id)
        {
            UserService.UserData u = Program.User.Get(id);

            IDMChannel c = await (await Program.Client.GetUserAsync(id)).CreateDMChannelAsync();

            if (u == null)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $"{c.Name} は登録されていません\n" +
                        $"**`!r [Username] [Password]`**")
                    .WithColor(Color.Red);
                await c.SendMessageAsync(embed: ebb.Build());
                return;
            }

            RiotLoginData logindata = new RiotLoginData()
            {
                username = Program.User.GetDecryptedUsername(u.encrypted_username),
                password = Program.User.GetDecryptedPassword(u.encrypted_password),
            };
            RiotUser user = new RiotUser(logindata);
            var res = await user.Authentication.AuthenticateWithCloud();

            if (!res.bIsAuthComplete)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $" ユーザーネーム : {logindata.username} , " +
                        $"パスワード : {logindata.password} のログインに失敗しました。")
                    .WithColor(Color.Red);
                await c.SendMessageAsync(embed: ebb.Build());
                return;
            }

            await user.Store.GetPlayerStore();

            if (user.Store.PlayerStore.BonusStore == null)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        "現在、ナイトマーケットは開催されていない可能性があります。")
                    .WithColor(Color.Red);
                await c.SendMessageAsync(embed: ebb.Build());
                return;
            }

            List<Embed> es = new List<Embed>();
            List<NightMarketOffer> offers = user.Store.PlayerStore.BonusStore.NightMarketOffers;

            foreach (var item in offers)
            {
                dynamic v = ValorantApiData.GetSkin(item.Offer.OfferID);
                EmbedBuilder eb = new EmbedBuilder()
                    .WithTitle(
                        ((string)v.data.GetProperty("displayName").GetRawText())
                            .Replace("\"", ""));
                eb.WithDescription($"{item.Offer.Cost.ValorantPointCost}\n");
                if (!string.IsNullOrEmpty(v.data.GetProperty("streamedVideo").GetString()))
                {
                    eb.Description +=
                        $"[動画]({v.data.GetProperty("streamedVideo").GetString()})";
                }
                eb.WithImageUrl(v.data.GetProperty("displayIcon").GetString());
                eb.WithColor(Color.Green);
                es.Add(eb.Build());
            }
            await c.SendMessageAsync(embeds: es.ToArray());
            return;
        }

        public static async Task ShowOfferEmbedsByCommand(SocketCommandContext Context)
        {
            UserService.UserData u = Program.User.Get(Context.User.Id);
            if (u == null)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $"{Context.User.Username} は登録されていません\n" +
                        $"**`!r [Username] [Password]`**")
                    .WithColor(Color.Red);
                await Context.Message.ReplyAsync(embed: ebb.Build());
                return;
            }

            RiotLoginData logindata = new RiotLoginData()
            {
                username = Program.User.GetDecryptedUsername(u.encrypted_username),
                password = Program.User.GetDecryptedPassword(u.encrypted_password),
            };
            RiotUser user = new RiotUser(logindata);
            var res = await user.Authentication.AuthenticateWithCloud();

            if (!res.bIsAuthComplete)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $" ユーザーネーム : {logindata.username} , " +
                        $"パスワード : {logindata.password} のログインに失敗しました。")
                    .WithColor(Color.Red);
                await Context.Message.ReplyAsync(embed: ebb.Build());
                return;
            }

            List<Embed> es = new List<Embed>();
            await user.Store.GetPlayerStore();
            List<string> offers = user.Store.PlayerStore.SkinsPanelLayout.SingleItemOffers;

            foreach (string item in offers)
            {
                dynamic v = ValorantApiData.GetSkin(item);
                EmbedBuilder eb = new EmbedBuilder()
                    .WithTitle(
                        ((string)v.data.GetProperty("displayName").GetRawText())
                            .Replace("\"", ""));
                if (!string.IsNullOrEmpty(v.data.GetProperty("streamedVideo").GetString()))
                {
                    eb.WithDescription(
                        $"[動画]({v.data.GetProperty("streamedVideo").GetString()})");
                }
                eb.WithImageUrl(v.data.GetProperty("displayIcon").GetString());
                Color color = Program.Tier.GetTierColor(v.data.GetProperty("displayName").GetString());
                eb.WithColor(color);
                int cost = Program.Tier.GetSkinPrice(color);
                if (cost > 0)
                {
                    eb.Title += $" > {cost} VP";
                }
                else
                {
                    eb.Title += $" > {-cost} ～ VP";
                }
                es.Add(eb.Build());
            }

            if (user.Store.PlayerStore.BonusStore != null)
            {
                EmbedBuilder eb = new EmbedBuilder()
                    .WithTitle("ナイトマーケット開催中")
                    .WithColor(Color.Purple)
                    .WithDescription("**`!n`** コマンドでナイトマーケットを確認できます");
                es.Add(eb.Build());
            }

            await Context.Message.ReplyAsync(embeds: es.ToArray());
            return;
        }
        public static async Task ShowNightMarketOfferEmbedsByCommand(SocketCommandContext Context)
        {
            UserService.UserData u = Program.User.Get(Context.User.Id);

            if (u == null)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $"{Context.User.Username} は登録されていません\n" +
                        $"**`!r [Username] [Password]`**")
                    .WithColor(Color.Red);
                await Context.Message.ReplyAsync(embed: ebb.Build());
                return;
            }

            RiotLoginData logindata = new RiotLoginData()
            {
                username = Program.User.GetDecryptedUsername(u.encrypted_username),
                password = Program.User.GetDecryptedPassword(u.encrypted_password),
            };
            RiotUser user = new RiotUser(logindata);
            var res = await user.Authentication.AuthenticateWithCloud();

            if (!res.bIsAuthComplete)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $" ユーザーネーム : {logindata.username} , " +
                        $"パスワード : {logindata.password} のログインに失敗しました。")
                    .WithColor(Color.Red);
                await Context.Message.ReplyAsync(embed: ebb.Build());
                return;
            }

            await user.Store.GetPlayerStore();

            if (user.Store.PlayerStore.BonusStore == null)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        "現在、ナイトマーケットは開催されていない可能性があります。")
                    .WithColor(Color.Red);
                await Context.Message.ReplyAsync(embed: ebb.Build());
                return;
            }

            List<Embed> es = new List<Embed>();
            List<NightMarketOffer> offers = user.Store.PlayerStore.BonusStore.NightMarketOffers;

            foreach (var item in offers)
            {
                dynamic v = ValorantApiData.GetSkin(item.Offer.OfferID);
                EmbedBuilder eb = new EmbedBuilder()
                    .WithTitle(
                        ((string)v.data.GetProperty("displayName").GetRawText())
                            .Replace("\"", ""));
                eb.WithDescription($"{item.Offer.Cost.ValorantPointCost}\n");
                if (!string.IsNullOrEmpty(v.data.GetProperty("streamedVideo").GetString()))
                {
                    eb.Description +=
                        $"[動画]({v.data.GetProperty("streamedVideo").GetString()})";
                }
                eb.WithImageUrl(v.data.GetProperty("displayIcon").GetString());
                eb.WithColor(Color.Green);
                es.Add(eb.Build());
            }
            await Context.Message.ReplyAsync(embeds: es.ToArray());
            return;
        }
    }
}
