﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Text;
using System.Net;
using Discord;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using SigBOT.Modules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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

        [Command("stoic")]
        public async Task Stoic()
        {
            var json = new WebClient().DownloadString("https://stoicquotesapi.com/v1/api/quotes/random");
            dynamic stuff = JsonConvert.DeserializeObject(json);
            string body = "\"" + stuff.body + "\"";
            string author = "***" + stuff.author + "***";
            {
                await ReplyAsync(body + "\n" + author);
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
                "teste" )); 
        }


        static int pingCount = 0;
        public static int replyStatus = 0;        
        

        #region VicesTypes
        public enum ViceType
        {
            Weed,
            Ciggarettes,
            Porn,
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
        #endregion

        #region ViceFunctions
        public static string endString = @". _ViceCounter v0.65_";
        [Command("vices")]
        public async Task ViceTracks()
        {
            var cmdId = Context.User.Id.ToString();
            var list = activeViceList.Where(x => x.userId == cmdId);
            string printedList = "";

            if (list == null || list.Count() < 1 )
            {
                await ReplyAsync("This user has no Vices being tracked : " + Context.User.Username);
                return;
            }
            foreach (var item in list)
            {
                TimeSpan timeSince = DateTime.Now.Subtract(item.dataInicio);
                printedList += (Context.User.Username + @"'s " + item.type.ToString("g") + " vice. " + timeSince.Days + " Days and " + (timeSince.TotalHours % 24).ToString("0.0") + " Hours since start." + "\n");
           }
            var builder = new EmbedBuilder()
            {
                Color = Color.Red,
                Description = "These are " + Context.User.Username + @"'s " + "vices."
            };
            builder.AddField(x =>
            {
                x.Name = Context.User.Username;
                x.Value = printedList;
                x.IsInline = false;
            });
            //await ReplyAsync(printedList);
            await ReplyAsync("", false, builder.Build());
            ViceController.Save();
        }
        [Command("lostweed")]
        public async Task LostWeed()
        {
            await LostAtVice(ViceType.Weed, Context);
        }
        [Command("lostcigar")]
        public async Task LostCigar()
        {
            await LostAtVice(ViceType.Ciggarettes, Context);
        }
        [Command("lostporn")]
        public async Task LostPorn()
        {
            await LostAtVice(ViceType.Porn, Context);
        }



        public async Task LostAtVice(ViceType vice, SocketCommandContext context)
        {            
            var cmdId = context.User.Id.ToString();
            var list = activeViceList.Where(x => x.userId == cmdId && x.type == vice);

            activeViceList.Remove(list.First());
            
            await ReplyAsync("Que vergonha, " + context.User.Username + ", mas relaxe, você sempre pode recomeçar.");
        }
        #endregion

        #region ViceController&Counter&Save_Classes
        [Serializable]
        public class ViceSave
        {
            public List<ViceCounter> vices;
            public ViceSave Init(List<ViceCounter> _vices) { vices = _vices; return this; }
            public ViceSave()
            {
            }
        }
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
        public static List<ViceCounter> activeViceList;
        public class ViceController
        {
            public static string fileName = "\\viceControllerdata.dat";
            public static string path = AppDomain.CurrentDomain.BaseDirectory;

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
        #endregion

    }
}
