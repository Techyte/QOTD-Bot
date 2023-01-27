using YamlDotNet.Serialization.NamingConventions;

namespace QOTD_Bot;

using DSharpPlus.Entities;

public class CommandsManager
{
    /// <summary>
    /// The reference to the program that is managing this question manager
    /// </summary>
    private Program _program;
    /// <summary>
    /// The reference to the question manager that the commands manager is using
    /// </summary>
    private QuestionManager _questionManager;

    public bool needToStop;
    
    public CommandsManager(Program program)
    {
        _program = program;
        _questionManager = program._questionManager;
    }

    public void CommandCycle()
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

            if (needToStop)
            {
                return;
            }
        }

        CommandCycle();
    }

    private void TestCommand(string command, string content)
    {
        switch (command)
        {
            case "-quietCut":
                _questionManager.RemoveQuestion(content, true);
                break;
            case "-remove":
                _questionManager.RemoveQuestion(content, false);
                break;
            case "-forceGen":
                ForceGeneric(content);
                break;
            case "-forceSpec":
                ForceSpecific(content);
                break;
            case "-clearForce":
                ClearForced();
                break;
            case "-timeDebug":
                TimeDebug();
                break;
            case "-stop":
                Stop();
                break;
            case "-sendMod":
                SendModMessage(content);
                break;
            case "-readout":
                if (string.IsNullOrEmpty(content))
                {
                    ReadoutQuestions();   
                }
                else
                {
                    ReadoutQuestions(content);
                }
                break;
            case "-clear":
                Console.Clear();
                break;
            case "-changeTimeHour":
                ChangeTimeHour(int.Parse(content));
                break;
            case "-changeTimeMinute":
                ChangeTimeMinute(int.Parse(content));
                break;
            case "-askQuestion":
                _questionManager.ForceAskQuestion();
                break;
            case "-resetTime":
                ResetTime();
                break;
        }
    }

    /// <summary>
    /// Resets the time that the question will be asked back to what is in the config file
    /// </summary>
    private void ResetTime()
    {
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        string path = Environment.GetEnvironmentVariable("QOTD-Config-Location");

        _program.configData = deserializer.Deserialize<ConfigData>(File.ReadAllText(path));
        Console.WriteLine("Any changes to the time the bot will go off at have been cleared");
    }

    /// <summary>
    /// Changes the hour that the question will be asked at newHour (24 hour time)
    /// </summary>
    /// <param name="newHour">The hour to change the time too</param>
    private void ChangeTimeHour(int newHour)
    {
        Console.WriteLine($"New hour is: {newHour}");
        _program.configData.Hour = newHour;
    }

    /// <summary>
    /// Changes the minute that the question will be asked at newHour (24 hour time)
    /// </summary>
    /// <param name="newMinute">The minute to change the time too</param>
    private void ChangeTimeMinute(int newMinute)
    {
        Console.WriteLine($"New minute is: {newMinute}");
        _program.configData.Minute = newMinute;
    }

    /// <summary>
    /// Reads out the questions that are yet to be asked
    /// </summary>
    private void ReadoutQuestions()
    {
        if (_questionManager.possibleQuestions.Count > 0)
        {
            Console.WriteLine($"All {_questionManager.possibleQuestions.Count} Question(s):");
            foreach (var question in _questionManager.possibleQuestions.Values)
            {
                Console.WriteLine($"Question from {question.Author.Username}: {question.Content}");
            }   
        }
        else
        {
            Console.WriteLine("No Questions");
        }
    }

    /// <summary>
    /// Reads out the questions submitted by the user with the same username as what you provide
    /// </summary>
    /// <param name="author">The name of the user</param>
    private void ReadoutQuestions(string author)
    {
        Console.WriteLine($"All questions from {author}:");

        List<DiscordMessage> messages = new List<DiscordMessage>();

        foreach (var question in _questionManager.possibleQuestions.Values)
        {
            if (question.Author.Username == author)
            {
                messages.Add(question);
            }
        }

        if (messages.Count > 0)
        {
            foreach (var question in messages)
            {
                Console.WriteLine($"Question: {question.Content}");   
            }
        }
        else
        {
            Console.WriteLine($"No questions from {author}");
        }
    }

    /// <summary>
    /// Sends a mod message
    /// </summary>
    /// <param name="message">The message that you want to send</param>
    private void SendModMessage(string message)
    {
        if (_program.discord.Guilds.TryGetValue(_program.configData.GuildId,
                out DiscordGuild guild))
        {
            if (guild.Channels.TryGetValue(_program.configData.ModChannelId,
                    out DiscordChannel channel))
            {
                Console.WriteLine($"Sending mod message: {message}");
                channel.SendMessageAsync(message);
            }
            else
            {
                Console.WriteLine("Could not find the channel");
            }
        }
        else
        {
            Console.WriteLine("Could not find the server");
        }
    }

    /// <summary>
    /// Stops the bot
    /// </summary>
    private void Stop()
    {
        Console.WriteLine("Stopping");
        needToStop = true;
    }

    /// <summary>
    /// Reads out the time the question will be asked at
    /// </summary>
    private void TimeDebug()
    {
        Console.WriteLine($"Target Hour: {_program.configData.Hour}. Current Hour: {DateTime.Now.Hour}");
        Console.WriteLine($"Target Minute: {_program.configData.Minute}. Current Minute: {DateTime.Now.Minute}");
    }

    private void SendForceChangedMessage()
    {
        if (_questionManager.possibleQuestions.TryGetValue(ulong.Parse(_questionManager.forcedMessage),
                out DiscordMessage message))
        {
            message.RespondAsync("This will no longer be the next forced question");
        }
    }

    private void ForceGeneric(string forcedGenericMessage)
    {
        if (_questionManager.wasForcesSpec)
        {
            SendForceChangedMessage();
        }

        _questionManager.wasForcesSpec = false;

        _questionManager.forcedMessage = $"a {forcedGenericMessage}";
        Console.WriteLine($"Forced the next question to: {forcedGenericMessage}");
    }

    private void ForceSpecific(string forcedSpecificMessage)
    {
        DiscordMessage finalForcedMessage = null;

        foreach (DiscordMessage message in _questionManager.possibleQuestions.Values)
        {
            if (message.Content == forcedSpecificMessage)
            {
                finalForcedMessage = message;
            }
        }

        if (finalForcedMessage != null)
        {
            _questionManager.forcedMessage = finalForcedMessage.Id.ToString();
            _questionManager.wasForcesSpec = true;
            finalForcedMessage.RespondAsync(
                "This question was forced to be the next Question. If you do not wish this to be the case please contact the administrator of this Bot");
            Console.WriteLine($"Forced the next question to: {forcedSpecificMessage}");
        }
        else
        {
            Console.WriteLine($"Could not find a message with the content: {forcedSpecificMessage}");
        }
    }

    private void ClearForced()
    {
        Console.WriteLine("Cleared the forced question");
        if (_questionManager.wasForcesSpec)
        {
            SendForceChangedMessage();
        }

        _questionManager.wasForcesSpec = false;

        _questionManager.forcedMessage = null;
    }

}