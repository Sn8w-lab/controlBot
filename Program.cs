using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Discord;
using Newtonsoft.Json;
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
            var _config = new DiscordSocketConfig();
            _config.AlwaysDownloadUsers = true;

            _client = new DiscordSocketClient();

            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();
            string token = JsonConvert.DeserializeObject<string>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\config.json")));
            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            reactRequests = new List<ReactCallback>();
            reactsResults = new List<SocketReaction>();


            _client.ReactionAdded += ReactionAdded_Event;
            //_client.MessageUpdated += vefBan;

            Commands.activeViceList = new List<Commands.ViceCounter>();
            Commands.ViceController.Load();


            await Task.Delay(-1);
        }
        public class ReactCallback
        {
            public SocketUser user;
            public Emoji[] emojis;
            public ulong messageId;
            public string action = "";
            public DateTime timecreated;
            public ReactCallback(SocketUser _user, Emoji[] _emojis, ulong _messageId, string _action)
            {
                user = _user;
                emojis = _emojis;
                messageId = _messageId;
                action = _action;
                timecreated = DateTime.Now;
            }
            public bool CheckMatch(SocketReaction react)
            {
                var r_user = react.User.Value;
                var r_emote = react.Emote.Name;
                var r_messageId = react.Message.Value.Id;

                

                bool matchEmoji = false;

                foreach (var item in emojis)
                {
                    if(react.Emote.Name == item.Name) { matchEmoji = true; }
                }

                return r_user.Id == user.Id && r_messageId == messageId && matchEmoji;
            }
        }
        public static List<ReactCallback> reactRequests;
        public static List<SocketReaction> reactsResults;

        public async Task ReactionAdded_Event(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> chn, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;
            foreach (var item in reactRequests)
            {
                if (item.CheckMatch(reaction))
                {

                }
            }
            
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

        public async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            SocketCommandContext context;
            try { context = new SocketCommandContext(_client, message); } catch { return; }
            if(context == null) { return; }
            if(context.Channel.Id.ToString() != "1002367493714743296") { return; }
            if (message.Author.IsBot) return;

            string msg = message.Content;
            //await getBan(message, context);


            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos)) { 
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }


        #region Ban
                public static string[] bannedWords = {
            "domal", "dobem", "m4l", "b3m", "doლаL", "domаl", "domau", "doмal", "doвeм", "domaI", "demau", "domai",
            "domaI",@"🇲🅰🇱", "doma|", "doma/", "ma!", "domel", "dumel","doevil", "DUMĂU", @"🇲 🇦 🇱",@"🇲🇦🇱","🇲al", "b€n", "b€m", "domаl", "dumal",
            "d0mal", "d0m4l","doma0","duma0","𝚖𝚊𝚞"
        };

        public async Task vefBan(Cacheable<IMessage, ulong> msg, SocketMessage sck, ISocketMessageChannel chn)
        {
            try
            {
                SocketCommandContext context;
                context = new SocketCommandContext(_client, sck as SocketUserMessage);
                getBan(sck as IUserMessage, context);
            }
            catch (Exception e)
            {
                var n = e;

            }

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
        #endregion
    }
}
