using DSharpPlus;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization.NamingConventions;

namespace QOTD_Bot
{
    public class Program
    {
        public DiscordClient discord;
        
        public ConfigData configData;

        public QuestionManager _questionManager;
        private CommandsManager _commandsManager;
        
        static void Main()
        {
            Program program = new Program();
        }

        private Program()
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            configData = deserializer.Deserialize<ConfigData>(File.ReadAllText("config.yaml"));
            
            /*
            configData.Token = Environment.GetEnvironmentVariable("token");
            configData.ChannelId = ulong.Parse(Environment.GetEnvironmentVariable("channelId"));
            configData.GuildId = ulong.Parse(Environment.GetEnvironmentVariable("guildId"));
            configData.Hour = int.Parse(Environment.GetEnvironmentVariable("hour"));
            configData.Minute = int.Parse(Environment.GetEnvironmentVariable("minute"));
            configData.ModChannelId = ulong.Parse(Environment.GetEnvironmentVariable("modChannelId"));*/
            
            Console.WriteLine($"Bot Token: {configData.Token}.");
            Console.WriteLine($"Id of the Server: {configData.GuildId}.");
            Console.WriteLine($"Id of the Channel: {configData.ChannelId}.");
            Console.WriteLine($"Question will be asked at {configData.Hour}:{configData.Minute} o'clock.");

            _questionManager = new QuestionManager(this);
            _commandsManager = new CommandsManager(this);
            
            MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = configData.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.None
            });

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Channel.Type == ChannelType.Private)
                {
                    _questionManager.QuestionAsked(e);
                }
            };

            discord.MessageDeleted += async (s, e) =>
            {
                if (e.Channel.Type == ChannelType.Private)
                {
                    _questionManager.QuestionDeleted(e);
                }
            };

            // Basically when the Bot has started up
            discord.GuildDownloadCompleted += _questionManager.GetQuestions;
            
            _questionManager.StartTimer();

            await discord.ConnectAsync();
            
            _commandsManager.CommandCycle();
        }
    }

    public class ConfigData
    {
        public string Token;
        public ulong GuildId;
        public ulong ChannelId;
        public ulong ModChannelId;
        public int Hour;
        public int Minute;

        public ConfigData()
        {
            Token = "";
            GuildId = 0;
            ChannelId = 0;
            ModChannelId = 0;
            Hour = 0;
            Minute = 0;
        }
    }
}