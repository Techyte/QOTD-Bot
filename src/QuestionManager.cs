﻿namespace QOTD_Bot;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class QuestionManager
{
    /// <summary>
    /// The dictionary that holds the list of current questions accessed via their message id
    /// </summary>
    public Dictionary<ulong, DiscordMessage> possibleQuestions;

    /// <summary>
    /// The reference to the program that is managing this question manager
    /// </summary>
    private Program _program;

    public bool wasForcesSpec;
    public string? forcedMessage;

    private bool hasAsked;
    
    public QuestionManager(Program program)
    {
        possibleQuestions = new Dictionary<ulong, DiscordMessage>();
        _program = program;
    }

    /// <summary>
    /// Starts the timer that checks every minute if its time to ask the question
    /// </summary>
    public void StartTimer()
    {
        // The timer that checks every minute to see if it is time to ask the question
        var aTimer = new System.Timers.Timer(60000);
            
        aTimer.Elapsed += (o, i) =>
        {
            CheckIfTime(_program.discord);
        };
            
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    /// <summary>
    /// Removes the question with the same content as questionContent
    /// </summary>
    /// <param name="questionContent">The content of the message that you want to delete</param>
    /// <param name="quiet">Whether the removal should be quiet</param>
    public void RemoveQuestion(string questionContent, bool quiet)
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
            if (!quiet)
            {
                message.RespondAsync(
                    "This question was removed by a moderator because it was deemed to be to inappropriate or not haha funny");   
            }
            Console.WriteLine($"Removed the question sent by {message.Author.Username}: '{message.Content}'");
            message.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));
            possibleQuestions.Remove(message.Id);
        }
        else
        {
            Console.WriteLine($"Could not remove the question '{questionContent}' because it could not be found");
        }
    }

    public void QuestionDeleted(MessageDeleteEventArgs e)
    {
        if (!e.Message.Author.IsBot)
        {
            possibleQuestions.Remove(e.Message.Id);
            Console.WriteLine($"Question from {e.Message.Author.Username} removed: {e.Message.Content}.");
        }
    }

    public void QuestionAsked(MessageCreateEventArgs e)
    {
        if (!e.Message.Author.IsBot)
        {
            e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("👍"));
            possibleQuestions.Add(e.Message.Id, e.Message);
            Console.WriteLine($"Received a question from {e.Author.Username}: {e.Message.Content}.");
        }
    }

    private void CheckIfTime(DiscordClient discord)
    {
        DateTime time = DateTime.Now;

        if (time.Hour == _program.configData.Hour && time.Minute == _program.configData.Minute)
        {
            AskQuestion(discord);
        }
    }

    /// <summary>
    /// Forces the question to be asked
    /// </summary>
    public void ForceAskQuestion()
    {
        AskQuestion(_program.discord);
        hasAsked = true;
    }
    
    /// <summary>
    /// Asks the question
    /// </summary>
    /// <param name="discord">The client that the question will be askedd from</param>
    private void AskQuestion(DiscordClient discord)
    {
        if(!hasAsked)
        {
            if (discord.Guilds.TryGetValue(_program.configData.GuildId, out DiscordGuild guild))
            {
                if (guild.Channels.TryGetValue(_program.configData.ChannelId, out DiscordChannel channel))
                {
                    if (possibleQuestions.Count == 0)
                    {
                        if(_program.configData.SendNoQuestionMessage)
                        {
                            channel.SendMessageAsync("There are no questions to be asked");
                        }
                        return;
                    }
                    
                    if (forcedMessage == null)
                    {
                        Random random = new Random();

                        int randomId = random.Next(0, possibleQuestions.Count);
                        List<ulong> IdList = new List<ulong>(possibleQuestions.Keys);

                        DiscordMessage message;
                        if (possibleQuestions.TryGetValue(IdList[randomId], out message))
                        {
                            DiscordMessage questionMessage = channel
                                .SendMessageAsync(message.Content).GetAwaiter()
                                .GetResult();

                            message.CreateReactionAsync(DiscordEmoji.FromUnicode("✔"));
                            message.RespondAsync(
                                $"This question was asked! Check it out here: {questionMessage.JumpLink}");
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
                                DiscordMessage questionMessage = channel
                                    .SendMessageAsync($"Question of the day is: {message.Content}").GetAwaiter()
                                    .GetResult();

                                message.CreateReactionAsync(DiscordEmoji.FromUnicode("✔"));
                                message.RespondAsync(
                                    $"This question was asked! Check it out here: {questionMessage.JumpLink}");
                                Console.WriteLine(
                                    $"A question was asked! Question content: {questionMessage.Content}.");
                                possibleQuestions.Remove(IdList[int.Parse(forcedMessage)]);
                            }
                        }
                        else
                        {
                            DiscordMessage questionMessage = channel
                                .SendMessageAsync($"Question of the day is: {testForSpec[1]}").GetAwaiter().GetResult();
                            Console.WriteLine($"A question was asked! Question content: {questionMessage.Content}.");
                        }

                        forcedMessage = null;
                    }
                }
            }
        }
        else
        {
            hasAsked = false;
        }
    }


    // Gets the questions that were already given to the bot before it was turned on
    public async Task GetQuestions(DiscordClient discord, GuildDownloadCompletedEventArgs e)
    {
        Console.WriteLine(
            "Started checking for messages, this may take some time depending on the number of members in your server.");

        if (discord.Guilds.TryGetValue(_program.configData.GuildId, out DiscordGuild guild))
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
                                Console.WriteLine(
                                    $"Found a question from {message.Author.Username}: {message.Content}.");
                            }
                        }
                    }
                }
            }
        }

        Console.WriteLine("Finished checking for messages.");
    }
}