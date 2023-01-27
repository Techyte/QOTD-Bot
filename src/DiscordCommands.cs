using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using QOTD_Bot;

public class DiscordCommands : BaseCommandModule
{
    private QuestionManager _questionManager;
    private Program _program;
    
    public DiscordCommands()
    {
        Console.WriteLine(Program.Instance);
        _questionManager = Program.Instance._questionManager;
        _program = Program.Instance;
    }
    
    [Command("readout")]
    public async Task Readout(CommandContext ctx)
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

    [Command("remove")]
    public async Task Remove(CommandContext ctx, string text)
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

    [Command("timeDebug")]
    public async Task TimeDebug(CommandContext ctx)
    {
        await ctx.Channel.SendMessageAsync($"Target Hour: {_program.configData.Hour}. Current Hour: {DateTime.Now.Hour}\n"+
                                           $"Target Minute: {_program.configData.Minute}. Current Minute: {DateTime.Now.Minute}");
    }

    [Command("stop")]
    public async Task Stop(CommandContext ctx)
    {
        if ((ctx.Member.Permissions & Permissions.Administrator) != 0)
        {
            await ctx.RespondAsync("Stopping");
            _program._commandsManager.needToStop = true;
        }
    }

    [Command("askQuestion")]
    public async Task askQuestion(CommandContext ctx)
    {
        if ((ctx.Member.Permissions & Permissions.ModerateMembers) != 0)
        {
            _program._questionManager.ForceAskQuestion();
        }
    }

    [Command("info")]
    public async Task Info(CommandContext ctx)
    {
        await ctx.RespondAsync("**The QOTD Bot!**\n\n"+
                               "DM me to ask a question\n"+
                               "Questions will be asked once a day in the dedicated QOTD channel\n"+
                               "For a list of other commands use !commandList\n"+
                               "To learn more specifics and how to set me up in your own server check out the github page:https://github.com/Techyte/QOTD-Bot\n\n"+
                               "*Created By Techyte*");
    }
    
    [Command("commandList")]
    public async Task CommandList(CommandContext ctx)
    {
        string allowReadouts = _program.configData.allowReadout
            ? "!readout: Reads out all of the questions yet to be asked\n"
            : string.Empty;

        string allowTimeModifications = _program.configData.allowTimeModifications
            ? "!changeTimeHour: Changes the hour that the question will be asked at (24 hour time)\n" +
              "!changeTimeMinute: Changes the minute that the question will be asked at\n"+
              "!resetTime: Clears any changes made to the time that the question will be asked at\n"
            : string.Empty;

        string allowRemovals = _program.configData.allowRemovals
            ? "!remove: Removes the question with the same content as what you give it\n"
            : string.Empty;
        
        await ctx.RespondAsync(
            "List of all commands allowed by the bots current settings:\n\n"+
            allowReadouts+
                               allowTimeModifications+
                               allowRemovals+
                               "!stop: Stops the bot (requires admin privileges)\n"+
                               "!info: Provides information about the bot\n"+
                               "!askQuestion: Forces the bot to ask the question (requires moderate members privileges)");
    }
}