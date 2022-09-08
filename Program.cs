using DSharpPlus;
using DSharpPlus.Entities;

namespace QOTD_Bot
{
    class Program
    {
        private static List<DiscordMessage> possibleQuestions;

        static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "MTAxNTQ2MjIwNDAwNDM4ODk2NQ.GTaIfF.shNacwrKs0DNUaEOVCido_z22Qa4fr0zhl929M",
                TokenType = TokenType.Bot
            });

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Channel.Type == ChannelType.Private)
                {
                    possibleQuestions.Add(e.Message);
                }

            };

            discord.MessageDeleted += async (s, e) =>
            {
                possibleQuestions.Remove(e.Message);
            };
            
            var aTimer = new System.Timers.Timer(60000);
            aTimer.Elapsed += (o, i) =>
            {
                Console.WriteLine("Checking");
                DateTime time = DateTime.Now;
                
                if (time.Hour == 12 && time.Minute == 0)
                {
                    if(discord.Guilds.TryGetValue(954307870869028906, out DiscordGuild guild))
                    {
                        if(guild.Channels.TryGetValue(1015459987000147998, out DiscordChannel channel))
                        {
                            Random random = new Random();

                            DiscordMessage message = possibleQuestions[random.Next(0, possibleQuestions.Count)];
                            
                            channel.SendMessageAsync(message.Content);
                            
                            message.RespondAsync("This question was asked! Check it out here: https://discord.com/channels/954307870869028906/1015459987000147998");
                        }
                    }
                }
            };
            
            if(discord.Guilds.TryGetValue(954307870869028906, out DiscordGuild guild))
            {
                foreach (var member in guild.Members)
                {
                    await member.Value.CreateDmChannelAsync();
                }
            }
            
            aTimer.AutoReset = true;
            
            aTimer.Enabled = true;
            
            await discord.ConnectAsync();
            
            await Task.Delay(-1);
        }
    }
}