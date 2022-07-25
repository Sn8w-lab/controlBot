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

namespace SigBOT.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        
        [Command("ping")]
        public async Task Ping()
        {
            pingCount++;
            if (pingCount < 5) { 
            await ReplyAsync("Pong");
            } else
            {
                pingCount = -99;
                await ReplyAsync("Ganhei, seu merda");
            }

        }
        static int pingCount = 0;



        public static int replyStatus = 0;

        [Command("re")]
        public async Task Re()
        {
            var message = await ReplyAsync("React Test");

            var v = new Emoji("✅");
            var x = new Emoji("❌");
            await message.AddReactionsAsync(new Emoji[] {v, x} );

            /*var task = GetReactMessage(new Emoji[]{v, x}, message, );
            task.Start();

            Program._client.ReactionAdded += GetReactMessage_Event;

            while(!task.IsCompleted) { Task.Delay(100); }

            ReplyAsync(task.Result.ToString());*/
        }

        public async Task<int> GetReactMessage(Emoji[] emoji, IMessage message)
        {
            return 0;
        }
        public async Task GetReactMessage_Event(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> chn, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;


        }

    }
}
