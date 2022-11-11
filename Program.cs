using System.Timers;
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
            
            Console.WriteLine($"Bot Token: {configData.Token}");
            Console.WriteLine($"Id of the Server: {configData.GuildId}");
            Console.WriteLine($"Id of the Channel: {configData.ChannelId}");
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
                    QuestionAsked(e);
                }
            };

            discord.MessageDeleted += async (s, e) =>
            {
                if (e.Channel.Type == ChannelType.Private)
                {
                    QuestionDeleted(e);
                }
            };

            // Basically when the Bot has started up
            discord.GuildDownloadCompleted += GetQuestions;
            
            // The timer that checks every minute to see if it is time to ask the question
            var aTimer = new System.Timers.Timer(60000);
            
            aTimer.Elapsed += (o, i) =>
            {
                CheckIfTime(discord);
            };
            
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            
            await discord.ConnectAsync();
            
            await Task.Delay(-1);
        }

        // Run when a question is deleted in a DM
        private static void QuestionDeleted(MessageDeleteEventArgs e)
        {
            if (!e.Message.Author.IsBot)
            {
                possibleQuestions.Remove(e.Message.Id);
                Console.WriteLine($"Question from {e.Message.Author} removed: {e.Message.Content}");   
            }
        }

        // Run when a question is asked in a DM
        private static void QuestionAsked(MessageCreateEventArgs e)
        {
            if (!e.Message.Author.IsBot)
            {
                e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("👍"));
                possibleQuestions.Add(e.Message.Id, e.Message);
                Console.WriteLine($"Received a question from {e.Author.Username}: {e.Message.Content}.");
            }
        }

        // Checks if it is time to ask the question
        private static void CheckIfTime(DiscordClient discord)
        {
            DateTime time = DateTime.Now;
                
            if (time.Hour == configData.hour && time.Minute == 00 && time.ToString("t") == "A")
            {
                AskQuestion(discord);
            }
        }

        // Asks the question
        private static void AskQuestion(DiscordClient discord)
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
                    Console.WriteLine($"A question was asked! Question content: {questionMessage.Content}.");
                }
            }
        }

        // Gets the questions that were already given to the bot before it was turned on
        private async static Task GetQuestions(DiscordClient discord, GuildDownloadCompletedEventArgs e)
        {
            Console.WriteLine("Started checking for messages this may take some time depending on the number of members in your server");
            
            if(discord.Guilds.TryGetValue(configData.GuildId, out DiscordGuild guild))
            {
                // Goes through every member in the server
                foreach (DiscordMember member in guild.GetAllMembersAsync().GetAwaiter().GetResult())
                {
                    if (!member.IsBot)
                    {
                        // Opens the members DM with the bot
                        DiscordChannel channel = member.CreateDmChannelAsync().GetAwaiter().GetResult();
                        foreach (DiscordMessage message in channel.GetMessagesAsync().GetAwaiter().GetResult())
                        {
                            if (!message.Author.IsBot)
                            {
                                // To make sure that the message has not been ticked (meaning that it has been asked)
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
            
            Console.WriteLine("Finished checking for messages");
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