using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Discord;

using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using SigBOT.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace SigBOT
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();
        
        public static DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string token = "MTAwMTA1OTIzNTU3NDA3MTMwNg.GR59rA.y8yDLbTNDQ_cdgTG59J38tBRVwy-aR-HiK8rMI";

            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            _client.ReactionAdded += ReactionAdded_Event;
            _client.MessageUpdated +=

            await Task.Delay(-1);
        }
        public async Task ReactionAdded_Event(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> chn, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;

            
        }
        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);

            return Task.CompletedTask;
        }
        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
        public static string[] bannedWords = {
            "domal", "dobem", "m4l", "b3m", "doლаL", "domаl", "domau", "doмal", "doвeм", "domaI", "demau", "domai",
            "domaI",@"🇲🅰🇱", "doma|", "doma/", "ma!", "domel", "dumel","doevil", "DUMĂU", @"🇲 🇦 🇱",@"🇲🇦🇱","🇲al", "b€n", "b€m", "domаl", "dumal",
            "d0mal", "d0m4l","doma0","duma0"
        };
        public async Task getBan(IMessage msg, IMessageChannel chn)
        {
            SocketCommandContext context;
            try { context = new SocketCommandContext(_client, msg as SocketUserMessage); } catch { return; }
            getBan(msg, context);
        }
        public async Task getBan(IMessage imsg, SocketCommandContext context)
        {
            string msg = imsg.Content;
            StringBuilder sb = new StringBuilder();
            char pastChr = '9';
            foreach (char c in msg)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    if (c != pastChr) { sb.Append(c); }

                }
                pastChr = c;
            }
            msg = sb.ToString();


            foreach (var item in bannedWords)
            {
                if (msg.ToLower().Contains(item))
                {
                    await context.Guild.GetUser(imsg.Author.Id).SetTimeOutAsync(new TimeSpan(0, 0, 1, 0));
                    context.Channel.SendMessageAsync("Um timeout pra ficar esperto, proibido antítese aqui " + imsg.Author.Mention);
                    return;
                }
            }

        }
        public async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            SocketCommandContext context;
            try { context = new SocketCommandContext(_client, message); } catch { return; }
            if(context == null) { return; }
            if (message.Author.IsBot) return;

            string msg = message.Content;

            await getBan(message, context);

            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos)) { 
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
