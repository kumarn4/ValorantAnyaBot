using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValNet;
using ValNet.Objects.Authentication;
using ValorantAnyaBot.Data;
using static ValNet.Objects.Store.NightMarket;

namespace ValorantAnyaBot.Command
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        [Command("r")]
        public async Task RegisterUser([Remainder] string arg)
        {
            await Context.Channel.TriggerTypingAsync();

            string[] args = arg.Split(" ");
            Program.Logger.Log(args[0] + "," + args[1]);

            if ((await Context.User.CreateDMChannelAsync()).Id !=
                    Context.Channel.Id)
            {
                await Context.Message.DeleteAsync();
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(" **`!r`** コマンドは DM のみで実行可能です")
                    .WithColor(Color.Red);
                await Context.Channel.SendMessageAsync(embed: ebb.Build());
                return;
            }

            if (args.Length != 2)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        " **`!r`** コマンドの引数が適切な数ではありません\n" +
                        " **`!r [Username] [Password]`** ")
                    .WithColor(Color.Red);
                await ReplyAsync(embed: ebb.Build());
                return;
            }

            RiotLoginData logindata = new RiotLoginData()
            {
                username = args[0],
                password = args[1],
            };
            RiotUser user = new RiotUser(logindata);
            AuthenticationStatus res = 
                await user.Authentication.AuthenticateWithCloud();

            if (!res.bIsAuthComplete)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $" ユーザーネーム : {logindata.username} , " +
                        $"パスワード : {logindata.password} のログインに失敗しました。")
                    .WithColor(Color.Red);
                await ReplyAsync(embed: ebb.Build());
                return;
            }

            Program.User.Set(Context.User.Id, logindata.username, logindata.password);
            EmbedBuilder eb = new EmbedBuilder()
                   .WithTitle("👍👍👍👍👍🥜")
                   .WithDescription(
                       $" ユーザーネーム : {logindata.username} , " +
                       $"パスワード : {logindata.password} を暗号化した状態で登録しました。")
                   .WithColor(Color.Green);
            await ReplyAsync(embed: eb.Build());
            return;
        }

        [Command("me")]
        public async Task ShowPersonalData()
        {
            await Context.Channel.TriggerTypingAsync();

            UserService.UserData u = Program.User.Get(Context.User.Id);
            if(u == null)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $"{Context.User.Username} は登録されていません\n" +
                        $"**`!r [Username] [Password]`**")
                    .WithColor(Color.Red);
                await ReplyAsync(embed: ebb.Build());
                return;
            }
            EmbedBuilder eb = new EmbedBuilder()
                   .WithTitle("👍👍👍👍👍🥜")
                   .WithDescription(
                       $"ログイン情報はランダムに生成された初期化ベクトルと鍵によって暗号化されています。\n" +
                       $" ユーザーネーム : {Program.User.GetDecryptedUsername(u.encrypted_username)} , " +
                       $"パスワード : {Program.User.GetDecryptedPassword(u.encrypted_password)}")
                   .WithColor(Color.Green);
            await ReplyAsync(embed: eb.Build());
            return;
        }

        [Command("s")]
        public async Task ShowOffers()
        {
            await Context.Channel.TriggerTypingAsync();

            UserService.UserData u = Program.User.Get(Context.User.Id);

            if (u == null)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $"{Context.User.Username} は登録されていません\n" +
                        $"**`!r [Username] [Password]`**")
                    .WithColor(Color.Red);
                await ReplyAsync(embed: ebb.Build());
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
                await ReplyAsync(embed: ebb.Build());
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
                eb.WithColor(Program.Tier.GetTierColor(v.data.GetProperty("displayName").GetString()));
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

            await ReplyAsync(embeds: es.ToArray());
            return;
        }

        [Command("n")]
        public async Task ShowNightMarket()
        {
            await Context.Channel.TriggerTypingAsync();

            UserService.UserData u = Program.User.Get(Context.User.Id);

            if (u == null)
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $"{Context.User.Username} は登録されていません\n" +
                        $"**`!r [Username] [Password]`**")
                    .WithColor(Color.Red);
                await ReplyAsync(embed: ebb.Build());
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
                await ReplyAsync(embed: ebb.Build());
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
                await ReplyAsync(embed: ebb.Build());
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
            await ReplyAsync(embeds: es.ToArray());
            return;
        }
    }
}
