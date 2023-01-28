using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization.NamingConventions;

namespace QOTD_Bot
{
    /// <summary>
    /// Main class that manages the bot
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main Instance of the program class
        /// </summary>
        public static Program Instance;
        
        /// <summary>
        /// The discord client that the bot uses
        /// </summary>
        public DiscordClient discord;
        
        /// <summary>
        /// The configuration data that the bot uses
        /// </summary>
        public ConfigData configData;

        /// <summary>
        /// The question manager that is currently in use
        /// </summary>
        public QuestionManager _questionManager;
        /// <summary>
        /// The commands manager that is currently in use
        /// </summary>
        public CommandsManager _commandsManager;

        /// <summary>
        /// The entry point of the bot
        /// </summary>
        static void Main()
        {
            Program program = new Program();
            Instance = program;
            program.Start();
        }

        /// <summary>
        /// The constructor that the bot uses for the main program class
        /// </summary>
        private Program()
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            string path = Directory.GetCurrentDirectory()+@"\config.yaml";

            configData = deserializer.Deserialize<ConfigData>(File.ReadAllText(path));
            
            ReadoutConfigData();

            _questionManager = new QuestionManager(this);
            _commandsManager = new CommandsManager(this);
        }

        /// <summary>
        /// The function that starts the bot after it has been configured
        /// </summary>
        private void Start()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// The function that starts the bot
        /// </summary>
        private async Task MainAsync()
        {
            discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = configData.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.None
            });
            
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            { 
                StringPrefixes = new[] { "!" }
            });

            commands.RegisterCommands<DiscordCommands>();

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
    
        public void ReloadConfigInformation()
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            string path = Directory.GetCurrentDirectory()+@"\config.yaml";

            configData = deserializer.Deserialize<ConfigData>(File.ReadAllText(path));
            ReadoutConfigData();
        }

        private void ReadoutConfigData()
        {
            Console.WriteLine($"Bot Token: {configData.Token}.");
            Console.WriteLine($"Id of the Server: {configData.GuildId}.");
            Console.WriteLine($"Id of the Channel: {configData.ChannelId}.");
            Console.WriteLine($"Question will be asked at {configData.Hour}:{configData.Minute} o'clock.");
            string willSendNoQuestionMessage = configData.SendNoQuestionMessage ? String.Empty : "not ";
            Console.WriteLine($"We will {willSendNoQuestionMessage}send a message when there are not questions");
            Console.WriteLine($"Readout command required permission: {configData.ReadoutPermission}");
            Console.WriteLine($"Time modification commands required permission: {configData.TimeModificationPermission}");
            Console.WriteLine($"Removal commands required permission: {configData.RemovalPermission}");
            Console.WriteLine($"Stop command required permission: {configData.StopPermission}");
            Console.WriteLine($"Ask Question command required permission: {configData.AskQuestionPermission}");
        }
    }

    /// <summary>
    /// The class that contains configuration data for the bot
    /// </summary>
    public class ConfigData
    {
        /// <summary>
        /// The token of the discord bot that the program uses
        /// </summary>
        public string Token;
        /// <summary>
        /// The id of the server the bot sends the message into
        /// </summary>
        public ulong GuildId;
        /// <summary>
        /// The id of the channel the bot sends the message into
        /// </summary>
        public ulong ChannelId;
        /// <summary>
        /// The id of the channel the bot sends mod messages into
        /// </summary>
        public ulong ModChannelId;
        /// <summary>
        /// The hour that the bot will ask the question at
        /// </summary>
        public int Hour;
        /// <summary>
        /// The minute that the bot will ask the question at
        /// </summary>
        public int Minute;
        /// <summary>
        /// Send a message when there are no questions to ask saying so
        /// </summary>
        public bool SendNoQuestionMessage;
        /// <summary>
        /// The required permission level for someone to run the readout command
        /// </summary>
        public Permissions ReadoutPermission;
        /// <summary>
        /// The required permission level for someone to run time modification commands
        /// </summary>
        public Permissions TimeModificationPermission;
        /// <summary>
        /// The required permission level for someone to run removal commands
        /// </summary>
        public Permissions RemovalPermission;
        /// <summary>
        /// The required permission level for someone to run the stop command
        /// </summary>
        public Permissions StopPermission;
        /// <summary>
        /// The required permission level for someone to run the ask question command
        /// </summary>
        public Permissions AskQuestionPermission;

        public ConfigData()
        {
            Token = "";
            GuildId = 0;
            ChannelId = 0;
            ModChannelId = 0;
            Hour = 0;
            Minute = 0;
            SendNoQuestionMessage = false;
            ReadoutPermission = Permissions.All;
            TimeModificationPermission = Permissions.All;
            RemovalPermission = Permissions.All;
            StopPermission = Permissions.All;
            AskQuestionPermission = Permissions.All;
        }
    }
}