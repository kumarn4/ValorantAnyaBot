using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using ValNet;
using ValNet.Objects.Authentication;
using ValorantAnyaBot.Services;

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
            await ValorantOfferService.ShowOfferEmbedsByCommand(Context);
        }

        [Command("n")]
        public async Task ShowNightMarket()
        {
            await Context.Channel.TriggerTypingAsync();
            await ValorantOfferService.ShowNightMarketOfferEmbedsByCommand(Context);
        }

        [Command("a")]
        public async Task SubscribeAutomateTask()
        {
            if (!Program.Auto.Add(this.Context.User.Id))
            {
                EmbedBuilder ebb = new EmbedBuilder()
                    .WithTitle("エラー")
                    .WithDescription(
                        $"{Context.User.Username} はすでに登録されています\n")
                    .WithColor(Color.Red);
                await ReplyAsync(embed: ebb.Build());
                return;
            }
            EmbedBuilder eb = new EmbedBuilder()
                    .WithTitle("👍👍👍👍👍🥜")
                    .WithDescription(
                        $"{Context.User.Username} を自動送信タスクに登録しました\n")
                    .WithColor(Color.Green);
            await ReplyAsync(embed: eb.Build());
            return;
        }

        [Command("l")]
        public async Task ShowCommandList()
        {
            EmbedBuilder eb = new EmbedBuilder()
                .WithTitle("🥜Command List🥜")
                .AddField(
                    "**!r** ``Username`` ``Password``",
                    "RiotGamesアカウントの Username と Password をBotに登録します\n" +
                    "- 登録する情報はすべて**暗号化**して保存します\n" +
                    "- **このコマンドはDM専用です**\n")
                .AddField(
                    "**!me**",
                    "登録したRiotGamesアカウントのログイン情報を開示します\n" +
                    "- **このコマンドはDM専用です**\n")
                .AddField(
                    "**!s**",
                    "ストアを表示します")
                .AddField(
                    "**!n**",
                    "ナイトマーケットのオファーを表示します")
                .AddField(
                    "**!a**",
                    "- **``重要`` このコマンドはテスト段階です**\n" +
                    "自動送信タスクに登録します\n" +
                    "これに登録すると毎日AM9:00ごろに自動的にDMにストアのオファーを送信します")
                .WithColor(Color.Blue);

            await ReplyAsync(embed: eb.Build());

        }
    }
}
