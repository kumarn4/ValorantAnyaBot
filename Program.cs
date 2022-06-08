using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LoggLibrary;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ValorantAnyaBot.Services;

namespace ValorantAnyaBot
{
    public class Program
    {
        public static DiscordSocketClient Client;
        public static CommandService Commands;
        public static IServiceProvider Services;
        public static Logger Logger;
        public static UserService User;
        public static ValorantSkinTierService Tier;
        public static AutomatedTaskService Auto;

        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            Client = new DiscordSocketClient();
            Commands = new CommandService();
            Services = new ServiceCollection().BuildServiceProvider();
            Logger = new Logger("Bot");
            User = new UserService();
            Tier = new ValorantSkinTierService();
            Auto = new AutomatedTaskService();

            Client.MessageReceived += OnClientMessageReceived;
            Client.Log += OnClientLog;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

            string key = KeyService.GetKey();

            if (key == null)
            {
                Logger.LogError("key.json was not found.");
                Logger.Log("key.json was created.");
                Logger.Log("Enter your Discord API Key in key.json.");
                return;
            }

            await Client.LoginAsync(TokenType.Bot, key);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task OnClientMessageReceived(SocketMessage arg)
        {
            SocketUserMessage m = (SocketUserMessage)arg;
            int argp = 0;

            if (m == null) return;
            if (m.Author.IsBot || m.Author.IsWebhook) return;
            if (m.HasMentionPrefix(Client.CurrentUser, ref argp) ||
                !m.HasCharPrefix('!', ref argp)) return;

            SocketCommandContext ctx = new SocketCommandContext(Client, m);
            IResult res = await Commands.ExecuteAsync(ctx, argp, Services);

            if (!res.IsSuccess)
                await m.ReplyAsync(res.ErrorReason);
        }

        private Task OnClientLog(LogMessage arg)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Info: Logger.Log(arg.Message); break;
                case LogSeverity.Warning: Logger.LogWarning(arg.Message); break;
                case LogSeverity.Error: Logger.LogError(arg.Message); break;
                case LogSeverity.Critical: Logger.LogError(arg.Message); break;
            }
            return Task.CompletedTask;
        }
    }
}