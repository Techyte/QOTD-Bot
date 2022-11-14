using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace QOTD_Bot
{
    class Program
    {
        private static ConfigData configData;
        
        private static Dictionary<ulong, DiscordMessage> possibleQuestions;

        private static bool wasForcesSpec;
        private static string? forcedMessage;

        static void Main()
        {
            configData = new ConfigData();
            possibleQuestions = new Dictionary<ulong, DiscordMessage>();
            
            configData.Token = Environment.GetEnvironmentVariable("token");
            configData.ChannelId = ulong.Parse(Environment.GetEnvironmentVariable("channelId"));
            configData.GuildId = ulong.Parse(Environment.GetEnvironmentVariable("guildId"));
            configData.Hour = int.Parse(Environment.GetEnvironmentVariable("hour"));
            configData.Minute = int.Parse(Environment.GetEnvironmentVariable("minute"));
            
            Console.WriteLine($"Bot Token: {configData.Token}.");
            Console.WriteLine($"Id of the Server: {configData.GuildId}.");
            Console.WriteLine($"Id of the Channel: {configData.ChannelId}.");
            Console.WriteLine($"Question will be asked at {configData.Hour}:{configData.Minute} o'clock.");
            
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
            
            CommandCycle();
        }

        private static void CommandCycle()
        {
            string msg = Console.ReadLine();

            if (msg != null)
            {
                string[] messageContent = msg.Split(" ", 2);

                if (messageContent.Length != 1)
                {
                    TestCommand(messageContent[0], messageContent[1]);   
                }
                else
                {
                    TestCommand(messageContent[0], string.Empty);
                }
            }
            
            CommandCycle();
        }

        private static void TestCommand(string command, string content)
        {
            switch (command)
            {
                case "-cut":
                    RemoveQuestion(content);
                    break;
                case "-forcegen":
                    ForceGeneric(content);
                    break;
                case "-forcespec":
                    ForceSpecific(content);
                    break;
                case "-clearforce":
                    ClearForced();
                    break;
                case "-timedebug":
                    TimeDebug();
                    break;
            }
        }

        private static void TimeDebug()
        {
            Console.WriteLine($"Target Hour: {configData.Hour}. Current Hour: {DateTime.Now.Hour}");
            Console.WriteLine($"Target Minute: {configData.Minute}. Current Minute: {DateTime.Now.Minute}");
        }

        private static void SendForceChangedMessage()
        {
            if (possibleQuestions.TryGetValue(ulong.Parse(forcedMessage), out DiscordMessage message))
            {
                message.RespondAsync("This will no longer be the next forced question");
            }
        }

        private static void ForceGeneric(string forcedGenericMessage)
        {
            if (wasForcesSpec)
            {
                SendForceChangedMessage();
            }
            wasForcesSpec = false;
            
            forcedMessage = $"a {forcedGenericMessage}";
            Console.WriteLine($"Forced the next question to: {forcedGenericMessage}");
        }

        private static void ForceSpecific(string forcedSpecificMessage)
        {
            DiscordMessage finalForcedMessage = null;
            
            foreach (DiscordMessage message in possibleQuestions.Values)
            {
                if (message.Content == forcedSpecificMessage)
                {
                    finalForcedMessage = message;
                }
            }

            if (finalForcedMessage != null)
            {
                forcedMessage = finalForcedMessage.Id.ToString();
                wasForcesSpec = true;
                finalForcedMessage.RespondAsync("This question was forced to be the next Question. If you do not wish this to be the case please contact the administrator of this Bot");
                Console.WriteLine($"Forced the next question to: {forcedSpecificMessage}");
            }
            else
            {
                Console.WriteLine($"Could not find a message with the content: {forcedSpecificMessage}");
            }
        }

        private static void ClearForced()
        {
            if (wasForcesSpec)
            {
                SendForceChangedMessage();
            }
            wasForcesSpec = false;
            
            forcedMessage = null;
        }

        private static void RemoveQuestion(string questionContent)
        {
            DiscordMessage message = null;
            
            foreach (var question in possibleQuestions.Values)
            {
                if (question.Content == questionContent)
                {
                    message = question;
                }
            }

            if (message != null)
            {
                Console.WriteLine($"Removing the question sent by {message.Author.Username}: '{message.Content}'");
                message.RespondAsync("This question was removed by a moderator because it was deemed to be to inappropriate or not haha funny");
                message.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));
                possibleQuestions.Remove(message.Id);
            }
            else
            {
                Console.WriteLine($"Could not remove the question '{questionContent}' because it could not be found");
            }
        }

        // Run when a question is deleted in a DM
        private static void QuestionDeleted(MessageDeleteEventArgs e)
        {
            if (!e.Message.Author.IsBot)
            {
                possibleQuestions.Remove(e.Message.Id);
                Console.WriteLine($"Question from {e.Message.Author} removed: {e.Message.Content}.");
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
            
            if (time.Hour == configData.Hour && time.Minute == configData.Minute)
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
                    if (forcedMessage == null)
                    {
                        Random random = new Random();

                        int randomId = random.Next(0, possibleQuestions.Count);
                        List<ulong> IdList = new List<ulong>(possibleQuestions.Keys);
                            
                        DiscordMessage message;
                        if(possibleQuestions.TryGetValue(IdList[randomId], out message))
                        {
                            DiscordMessage questionMessage = channel.SendMessageAsync($"Question of the day is: {message.Content}").GetAwaiter().GetResult();

                            message.CreateReactionAsync(DiscordEmoji.FromUnicode("✔"));
                            message.RespondAsync($"This question was asked! Check it out here: {questionMessage.JumpLink}");
                            Console.WriteLine($"A question was asked! Question content: {questionMessage.Content}.");
                            possibleQuestions.Remove(IdList[randomId]);
                        }
                    }
                    else
                    {
                        string[] testForSpec = forcedMessage.Split(" ", 2);

                        if (testForSpec.Length == 1)
                        {
                            List<ulong> IdList = new List<ulong>(possibleQuestions.Keys);
                            
                            DiscordMessage message;
                            if (possibleQuestions.TryGetValue(IdList[int.Parse(forcedMessage)], out message))
                            {
                                DiscordMessage questionMessage = channel.SendMessageAsync($"Question of the day is: {message.Content}").GetAwaiter().GetResult();

                                message.CreateReactionAsync(DiscordEmoji.FromUnicode("✔"));
                                message.RespondAsync($"This question was asked! Check it out here: {questionMessage.JumpLink}");
                                Console.WriteLine($"A question was asked! Question content: {questionMessage.Content}.");
                                possibleQuestions.Remove(IdList[int.Parse(forcedMessage)]);   
                            }
                        }
                        else
                        {
                            DiscordMessage questionMessage = channel.SendMessageAsync($"Question of the day is: {testForSpec[1]}").GetAwaiter().GetResult();
                            Console.WriteLine($"A question was asked! Question content: {questionMessage.Content}.");
                        }
                        
                        forcedMessage = null;
                    }
                }
            }
        }

        // Gets the questions that were already given to the bot before it was turned on
        private async static Task GetQuestions(DiscordClient discord, GuildDownloadCompletedEventArgs e)
        {
            Console.WriteLine("Started checking for messages, this may take some time depending on the number of members in your server.");
            
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
            
            Console.WriteLine("Finished checking for messages.");
        }
    }

    [Serializable]
    class ConfigData
    {
        public string Token;
        public ulong GuildId;
        public ulong ChannelId;
        public int Hour;
        public int Minute;

        public ConfigData()
        {
            Token = "";
            GuildId = 0;
            ChannelId = 0;
            Hour = 0;
            Minute = 0;
        }
    }
}