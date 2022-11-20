namespace QOTD_Bot;

using DSharpPlus.Entities;

public class CommandsManager
{

    private Program _program;
    private QuestionManager _questionManager;

    private bool needToStop;
    
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
            case "-quietcut":
                _questionManager.RemoveQuestion(content, true);
                break;
            case "-cut":
                _questionManager.RemoveQuestion(content, false);
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
            case "-stop":
                Stop();
                break;
            case "-sendmod":
                SendModMessage(content);
                break;
            case "-readout":
                ReadoutQuestions();
                break;
        }
    }

    private void ReadoutQuestions()
    {
        foreach (var question in _questionManager.possibleQuestions.Values)
        {
            Console.WriteLine($"Question from {question.Author.Username}: {question.Content}");
        }
    }

    private void SendModMessage(string message)
    {
        if (_program.discord.Guilds.TryGetValue(_program.configData.GuildId,
                out DiscordGuild guild))
        {
            if (guild.Channels.TryGetValue(_program.configData.ModChannelId,
                    out DiscordChannel channel))
            {
                channel.SendMessageAsync(message);
            }
        }
    }

    private void Stop()
    {
        needToStop = true;
    }

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
        if (_questionManager.wasForcesSpec)
        {
            SendForceChangedMessage();
        }

        _questionManager.wasForcesSpec = false;

        _questionManager.forcedMessage = null;
    }

}