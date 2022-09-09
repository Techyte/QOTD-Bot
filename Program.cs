using System.Text.Json;
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
            
            configData.Token = Environment.GetEnvironmentVariable("token");
            configData.ChannelId = ulong.Parse(Environment.GetEnvironmentVariable("channelId"));
            configData.GuildId = ulong.Parse(Environment.GetEnvironmentVariable("guildId"));
            
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = configData.Token,
                TokenType = TokenType.Bot
            });

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Channel.Type == ChannelType.Private)
                {
                    possibleQuestions.Add(e.Message.Id, e.Message);
                }

            };

            discord.MessageDeleted += async (s, e) =>
            {
                Console.WriteLine("Ready");
                possibleQuestions.Remove(e.Message.Id);
            };

            discord.GuildDownloadCompleted += GetQuestions;
            
            var aTimer = new System.Timers.Timer(60000);
            aTimer.Elapsed += (o, i) =>
            {
                Console.WriteLine("Checking");
                DateTime time = DateTime.Now;
                
                if (time.Hour == 12 && time.Minute == 0)
                {
                    Console.WriteLine("Its time");
                    if(discord.Guilds.TryGetValue(configData.GuildId, out DiscordGuild guild))
                    {
                        if(guild.Channels.TryGetValue(configData.ChannelId, out DiscordChannel channel))
                        {
                            Random random = new Random();

                            DiscordMessage message;
                            possibleQuestions.TryGetValue((ulong)random.Next(0, possibleQuestions.Count), out message);
                            
                            DiscordMessage questionMessage = channel.SendMessageAsync(message.Content).GetAwaiter().GetResult();

                            message.CreateReactionAsync(DiscordEmoji.FromUnicode("✔"));
                            message.RespondAsync($"This question was asked! Check it out here: {questionMessage.JumpLink}");
                        }
                    }
                }
            };
            
            aTimer.AutoReset = true;
            
            aTimer.Enabled = true;
            
            await discord.ConnectAsync();
            
            await Task.Delay(-1);
        }

        private static Task GetQuestions(DiscordClient discord, GuildDownloadCompletedEventArgs e)
        {
            Console.WriteLine("Checking for previously send questions");
            if(discord.Guilds.TryGetValue(configData.GuildId, out DiscordGuild guild))
            {
                foreach (DiscordMember member in guild.GetAllMembersAsync().GetAwaiter().GetResult())
                {
                    DiscordChannel channel = member.CreateDmChannelAsync().GetAwaiter().GetResult();
                    foreach (DiscordMessage message in channel.GetMessagesAsync(100).GetAwaiter().GetResult())
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
                                Console.WriteLine("Found a question");
                            }
                        }
                    }
                }
            }
            
            throw new NotImplementedException();
        }
    }

    [Serializable]
    class ConfigData
    {
        public string Token;
        public ulong GuildId;
        public ulong ChannelId;

        public ConfigData()
        {
            Token = "";
            GuildId = 0;
            ChannelId = 0;
        }
    }
}