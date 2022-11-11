using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace QOTD_Bot
{
    class Program
    {
        private static ConfigData configData;
        
        private static Dictionary<ulong, DiscordMessage> possibleQuestions;

        static void Main()
        {
            configData = new ConfigData();
            possibleQuestions = new Dictionary<ulong, DiscordMessage>();
            
            configData.Token = Environment.GetEnvironmentVariable("token");
            configData.ChannelId = ulong.Parse(Environment.GetEnvironmentVariable("channelId"));
            configData.GuildId = ulong.Parse(Environment.GetEnvironmentVariable("guildId"));
            configData.hour = int.Parse(Environment.GetEnvironmentVariable("hour"));
            
            Console.WriteLine($"Question will be asked at {configData.hour} o'clock");
            
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = configData.Token,
                TokenType = TokenType.Bot,
            });

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Channel.Type == ChannelType.Private)
                {
                    if (!e.Message.Author.IsBot)
                    {
                        e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("👍"));
                        possibleQuestions.Add(e.Message.Id, e.Message);
                        Console.WriteLine($"Received a question from {e.Author.Username}: {e.Message.Content}.");
                    }
                }
            };

            discord.MessageDeleted += async (s, e) =>
            {
                possibleQuestions.Remove(e.Message.Id);
                Console.WriteLine($"Question from {e.Message.Author} removed: {e.Message.Content}");
            };

            discord.GuildDownloadCompleted += GetQuestions;
            
            var aTimer = new System.Timers.Timer(60000);
            aTimer.Elapsed += (o, i) =>
            {
                DateTime time = DateTime.Now;
                
                if (time.Hour == configData.hour && time.Minute == 00 && time.ToString("t") == "A")
                {
                    if(discord.Guilds.TryGetValue(configData.GuildId, out DiscordGuild guild))
                    {
                        if(guild.Channels.TryGetValue(configData.ChannelId, out DiscordChannel channel))
                        {
                            Random random = new Random();

                            int randomId = random.Next(0, possibleQuestions.Count);
                            List<ulong> IdList = new List<ulong>(possibleQuestions.Keys);
                            
                            DiscordMessage message;
                            possibleQuestions.TryGetValue(IdList[randomId], out message);
                            
                            DiscordMessage questionMessage = channel.SendMessageAsync($"Question of the day is: {message.Content}").GetAwaiter().GetResult();

                            message.CreateReactionAsync(DiscordEmoji.FromUnicode("✔"));
                            message.RespondAsync($"This question was asked! Check it out here: {questionMessage.JumpLink}");
                            Console.WriteLine($"A question was asked! Question content: {questionMessage.JumpLink}.");
                        }
                    }
                }
            };
            
            aTimer.AutoReset = true;
            
            aTimer.Enabled = true;
            
            await discord.ConnectAsync();
            
            await Task.Delay(-1);
        }

        private async static Task GetQuestions(DiscordClient discord, GuildDownloadCompletedEventArgs e)
        {
            if(discord.Guilds.TryGetValue(configData.GuildId, out DiscordGuild guild))
            {
                foreach (DiscordMember member in guild.GetAllMembersAsync().GetAwaiter().GetResult())
                {
                    if (!member.IsBot)
                    {
                        DiscordChannel channel = member.CreateDmChannelAsync().GetAwaiter().GetResult();
                        foreach (DiscordMessage message in channel.GetMessagesAsync().GetAwaiter().GetResult())
                        {
                            if (!message.Author.IsBot)
                            {
                                int reactions = 0;
                                foreach (DiscordReaction reaction in message.Reactions)
                                {
                                    if (reaction.Emoji.ToString() == "👍")
                                    {
                                        reactions++;
                                    }
                                }

                                if (reactions == message.Reactions.Count)
                                {
                                    possibleQuestions.Add(message.Id, message);
                                    Console.WriteLine($"Found a question from {message.Author.Username}: {message.Content}.");
                                }
                            }
                        }   
                    }
                }
            }
        }
    }

    [Serializable]
    class ConfigData
    {
        public string Token;
        public ulong GuildId;
        public ulong ChannelId;
        public int hour;

        public ConfigData()
        {
            Token = "";
            GuildId = 0;
            ChannelId = 0;
            hour = 0;
        }
    }
}