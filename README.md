# QOTD Discord Bot
 
Wrote a whole nice README and it got deleted so here is the jist of it:

- Use this howver you want (See LICENCE for more details)
- You DM the bot to give it a question
- Delete a message to remove it from the bots list of questions
- Fully modular command system (operated from the console)
- You need to set up the config file with stuff like the bot token, server id, channel id and the time you want the questions to be asked (config.yaml)
- You will need a discord application to use this, set one up here: https://discord.com/developers/applications
- I don't provide a way of hosting but if you want a suggestion I am using AWS EC2 Windows 10 Instance
- If you care enough I am using DSharpPlus to make this (the dlls should be included with the source code)
- If you need any help with anything email me at contact@techyte.net

List of current commands:

- '-cut', removes a question based with the same content as the text you give it, notifys the person that asked the question
- '-quietcut', same as -cut but does not notify the person that asked the question
- '-forcegen', forces the next question to be the text you give it
- '-forcespec', forces the next question to be a with the same content as the text you give it, notifys the person that asked the question
- '-clearforce', clears forced question and uses a regular random question
- '-timedebug', prints out the target time to ask the question and the current time
- '-stop', stops the program and turns off the bot
- '-sendmod', sends a message with the content of what you give it to the mod channel
- '-readout', prints out every current possible question plus who asked it
