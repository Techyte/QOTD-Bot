using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using QOTD_Bot;
using YamlDotNet.Serialization.NamingConventions;

public class DiscordCommands : BaseCommandModule
{
    private QuestionManager _questionManager;
    private Program _program;
    
    public DiscordCommands()
    {
        _questionManager = Program.Instance._questionManager;
        _program = Program.Instance;
    }
    
    [Command("readout")]
    public async Task Readout(CommandContext ctx)
    {
        if((ctx.Member?.Permissions & _program.configData.ReadoutPermission) != 0)
        {
            Console.WriteLine("Reading out questions");
            await ctx.Channel.SendMessageAsync(
                "Started checking for messages, this may take some time depending on the number of members in your server.");

            string response = "All Questions:\n";

            foreach (var question in _questionManager.possibleQuestions.Values)
            {
                response = $"{response}Question from {question.Author.Username}: {question.Content}\n";
            }

            await ctx.Channel.SendMessageAsync(response);
        }
        else
        {
            NotAllowedToPerformAction(ctx);
        }
    }

    [Command("remove")]
    public async Task Remove(CommandContext ctx, string text)
    {
        if((ctx.Member?.Permissions & _program.configData.RemovalPermission) != 0)
        {
            DiscordMessage message = null;

            foreach (var question in _questionManager.possibleQuestions.Values)
            {
                if (question.Content == text)
                {
                    message = question;
                }
            }

            if (message != null)
            {
                await message.RespondAsync(
                    "This question was removed by a moderator because it was deemed to be to inappropriate or not haha funny");
                await ctx.RespondAsync($"Removed the question sent by {message.Author.Username}: '{message.Content}'");
                await message.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));
                _questionManager.possibleQuestions.Remove(message.Id);
            }
            else
            {
                await ctx.RespondAsync($"Could not remove the question '{text}' because it could not be found");
            }
        }
        else
        {
            NotAllowedToPerformAction(ctx);
        }
    }

    [Command("quietRemove")]
    public async Task QuietRemove(CommandContext ctx, string text)
    {
        if((ctx.Member?.Permissions & _program.configData.RemovalPermission) != 0)
        {
            DiscordMessage message = null;

            foreach (var question in _questionManager.possibleQuestions.Values)
            {
                if (question.Content == text)
                {
                    message = question;
                }
            }

            if (message != null)
            {
                await ctx.RespondAsync($"Removed the question sent by {message.Author.Username}: '{message.Content}'");
                await message.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));
                _questionManager.possibleQuestions.Remove(message.Id);
            }
            else
            {
                await ctx.RespondAsync($"Could not remove the question '{text}' because it could not be found");
            }
        }
        else
        {
            NotAllowedToPerformAction(ctx);
        }
    }

    [Command("removeAllBy")]
    public async Task RemoveAllBy(CommandContext ctx, string text)
    {
        if((ctx.Member?.Permissions & _program.configData.RemovalPermission) != 0)
        {
            List<DiscordMessage> questions = new List<DiscordMessage>();

            await ctx.RespondAsync($"Removing all questions submitted by {text}");
            foreach (var question in _questionManager.possibleQuestions.Values)
            {
                if (question.Author.Username == text)
                {
                    questions.Add(question);
                }
            }

            foreach (var question in questions)
            {
                await question.RespondAsync(
                    "This question was removed by a moderator because it was deemed to be to inappropriate or not haha funny");
                await ctx.RespondAsync($"Removed the question sent by {question.Author.Username}: '{question.Content}'");
                await question.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));
                _questionManager.possibleQuestions.Remove(question.Id);
            }
        }
        else
        {
            NotAllowedToPerformAction(ctx);
        }
    }

    [Command("timeDebug")]
    public async Task TimeDebug(CommandContext ctx)
    {
        await ctx.Channel.SendMessageAsync(
            $"Target Hour: {_program.configData.Hour}. Current Hour: {DateTime.Now.Hour}\n" +
            $"Target Minute: {_program.configData.Minute}. Current Minute: {DateTime.Now.Minute}");
    }

    [Command("changeTimeHour")]
    public async Task ChangeTimeHour(CommandContext ctx, string text)
    {
        if((ctx.Member?.Permissions & _program.configData.TimeModificationPermission) != 0)
        {
            await ctx.RespondAsync($"New hour is: {text}");
            _program.configData.Hour = int.Parse(text);
        }
        else
        {
            NotAllowedToPerformAction(ctx);
        }
    }

    [Command("changeTimeMinute")]
    public async Task ChangeTimeMinute(CommandContext ctx, string text)
    {
        if ((ctx.Member?.Permissions & _program.configData.TimeModificationPermission) != 0)
        {
            await ctx.RespondAsync($"New minute is: {text}");
            _program.configData.Minute = int.Parse(text);   
        }
        else
        {
            NotAllowedToPerformAction(ctx);
        }
    }

    [Command("resetTime")]
    public async Task ResetTime(CommandContext ctx)
    {
        if((ctx.Member?.Permissions & _program.configData.TimeModificationPermission) != 0)
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string path = Environment.GetEnvironmentVariable("QOTD-Config-Location");

            _program.configData = deserializer.Deserialize<ConfigData>(File.ReadAllText(path));
            await ctx.RespondAsync("Any changes to the time the bot will go off at have been cleared");
        }
        else
        {
            NotAllowedToPerformAction(ctx);
        }
    }

    [Command("stop")]
    public async Task Stop(CommandContext ctx)
    {
        if ((ctx.Member?.Permissions & _program.configData.StopPermission) != 0)
        {
            await ctx.RespondAsync("Stopping");
            Environment.Exit(0);
        }
        else
        {
            NotAllowedToPerformAction(ctx);
        }
    }

    [Command("askQuestion")]
    public async Task askQuestion(CommandContext ctx)
    {
        if ((ctx.Member?.Permissions & _program.configData.AskQuestionPermission) != 0)
        {
            _program._questionManager.ForceAskQuestion();
        }
        else
        {
            NotAllowedToPerformAction(ctx);
        }
    }

    [Command("info")]
    public async Task Info(CommandContext ctx)
    {
        await ctx.RespondAsync("**The QOTD Bot!**\n\n"+
                               "DM me to ask a question\n"+
                               "Questions will be asked once a day in the dedicated QOTD channel\n"+
                               "For a list of all commands, use !commandList\n"+
                               "To learn more specifics and how to set me up in your own server check out the github page:https://github.com/Techyte/QOTD-Bot\n\n"+
                               "*Created By Techyte*");
    }

    [Command("commandList")]
    public async Task CommandList(CommandContext ctx)
    {
        string allowReadouts = (ctx.Member?.Permissions & _program.configData.ReadoutPermission) != 0
            ? "!readout: Reads out all of the questions yet to be asked\n"
            : string.Empty;

        string allowTimeModifications = (ctx.Member?.Permissions & _program.configData.TimeModificationPermission) != 0
            ? "!changeTimeHour: Changes the hour that the question will be asked at (24 hour time)\n" +
              "!changeTimeMinute: Changes the minute that the question will be asked at\n"+
              "!resetTime: Clears any changes made to the time that the question will be asked at\n"
            : string.Empty;

        string allowRemovals = (ctx.Member?.Permissions & _program.configData.RemovalPermission) != 0
            ? "!remove: Removes the question with the same content as what you give it\n"+
              "!quietRemove: Removes the question with the same content as what you give it without telling the person that asked it\n"+
              "!removeAllBy: Removes all questions submitted by the user you provide\n"
            : string.Empty;

        string allowStop = (ctx.Member?.Permissions & _program.configData.StopPermission) != 0
            ? "!stop: Stops the bot\n"
            : String.Empty;

        string allowAsk = (ctx.Member?.Permissions & _program.configData.AskQuestionPermission) != 0
            ? "!askQuestion: Forces the bot to ask a question\n"
            : String.Empty;
        
        await ctx.RespondAsync(
            "List of all commands that you can use:\n\n"+
            allowReadouts+
                               allowTimeModifications+
                               allowRemovals+
                               allowStop+
                               allowAsk+
                               "!info: Provides information about the bot\n"+
                               "!commandList: Reads out all commands that the person that used it can use");
    }

    private void NotAllowedToPerformAction(CommandContext ctx)
    {
        ctx.RespondAsync("You do not have permission to perform this action");
    }
}