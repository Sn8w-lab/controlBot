using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Channels;
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
        public async Task GetReactMessage_Event(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> chn, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;
        }

        [Command("react")]
        public async Task React()
        {
            var message = await ReplyAsync("React Test");
            var v = new Emoji("✅");
            var x = new Emoji("❌");
            var emojis = new Emoji[] { v, x };
            await message.AddReactionsAsync(emojis);

            Program.reactRequests.Add(new Program.ReactCallback(
                Context.User,
                emojis,
                message.Id,
                "teste"
                )); 
        }


        static int pingCount = 0;
        public static int replyStatus = 0;        
        public class ViceCounter
        {
            public DateTime dataInicio, dataPerda;
            public string userId, userName;
            public ViceType type;

            public void Init(ViceType vice, SocketUser user)
            {
                type = vice;
                userId = user.Id.ToString();
                userName = user.Username;
                dataInicio = DateTime.Now;

            }
            public ViceCounter()
            {
            }
        }
        string weedPhrase = "decided to STOP SMOKING WEED at : ";
        string cigarPhrase = "decided to STOP SMOKING CIGARS at : ";
        string pornPhrase = "decided to STOP WATCHING PORN at : ";
        [Command("weed")]
        public async Task Weed()
        {
            var cmdId = Context.User.Id.ToString();
            var list = activeViceList.Where(x => x.userId == cmdId && x.type == ViceType.Weed);

            if (list.Count() > 1)
            {
                await ReplyAsync("This user's vice is already being tracked.");
                return;
            }

            await ReplyAsync(Context.User.Mention + weedPhrase + DateTime.Now + endString);
            if (activeViceList == null) { activeViceList = new List<ViceCounter>(); }
            var vice = new ViceCounter();
            vice.Init(ViceType.Weed, Context.User);
            activeViceList.Add(vice);
            ViceController.Save();
        }
        [Command("cigar")]
        public async Task Cigar()
        {
            var cmdId = Context.User.Id.ToString();
            var list = activeViceList.Where(x => x.userId == cmdId && x.type == ViceType.Ciggarettes);

            if (list.Count() > 1)
            {
                await ReplyAsync("This user's vice is already being tracked.");
                return;
            }

            await ReplyAsync(Context.User.Mention + cigarPhrase + DateTime.Now + endString);
            if (activeViceList == null) { activeViceList = new List<ViceCounter>(); }
            var vice = new ViceCounter();
            vice.Init(ViceType.Ciggarettes, Context.User);
            activeViceList.Add(vice);
            ViceController.Save();
        }
        [Command("porn")]
        public async Task Porn()
        {
            var cmdId = Context.User.Id.ToString();
            var list = activeViceList.Where(x => x.userId == cmdId && x.type == ViceType.Porn);

            if (list.Count() > 1)
            {
                await ReplyAsync("This user's vice is already being tracked.");
                return;
            }

            await ReplyAsync(Context.User.Mention + pornPhrase + DateTime.Now +endString);
            if (activeViceList == null) { activeViceList = new List<ViceCounter>(); }
            var vice = new ViceCounter();
            vice.Init(ViceType.Porn, Context.User);
            activeViceList.Add(vice);
            ViceController.Save();
        }

        public static string endString = " .  ViceCounter v0.12";
        [Command("vc")]
        public async Task ViceTracks()
        {
            var cmdId = Context.User.Id.ToString();
            var list = activeViceList.Where(x => x.userId == cmdId);

            if(list == null || list.Count() < 1 )
            {
                await ReplyAsync("This user has no Vices being tracked : " + Context.User.Username);
                return;
            }
            foreach (var item in list)
            {
                TimeSpan timeSince = DateTime.Now.Subtract(item.dataInicio);
                await ReplyAsync("Tracking :  " + Context.User.Username+ @"'s " + item.type.ToString("g") + " vice. "
                     + timeSince.Days + " Days and " + (timeSince.TotalHours % 24).ToString("0.00") + " Hours since start.");
                //await ReplyAsync(@"Did you lose ? if you did, type  '!lost' + cigar, weed or porn, Ex: !lostcigar .");
            }
            ViceController.Save();
        }

        [Serializable]
        public class ViceSave
        {
            public List<ViceCounter> vices;
            public ViceSave Init(List<ViceCounter> _vices) { vices = _vices; return this; }
            public ViceSave()
            {
            }
        }

        public static List<ViceCounter> activeViceList;
        public class ViceController
        {
            public static string fileName = "viceControllerdata.dat";
            public static string path = @"C:\Users\Gabe\source\repos\SigBOT\bin\Debug\netcoreapp3.1\";

            public static void Save()
            {
                Serializer.SerializeObject(new ViceSave().Init(activeViceList), path + fileName);
            }
            public static void Load()
            {
                try { 
                    var save = Serializer.DeSerializeObject<ViceSave>(path + fileName);
                    activeViceList = save.vices;
                } 
                catch(Exception) { };
            }
        }
        public enum ViceType
        {
            Weed,
            Ciggarettes,
            Porn,
        }
    }
}
