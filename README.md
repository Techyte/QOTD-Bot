# QOTD Discord Bot
 
 THIS README IS UP TO DATE AS OF QOTD v1.4
 
Wrote a whole nice README and it got deleted so here is the jist of it:

- Use this howver you want (See LICENCE for more details)
- You DM the bot to give it a question
- Delete a message to remove it from the bots list of questions
- Fully modular command system (operated from the console)
- You need to set up the config file with stuff like the bot token, server id, channel id and the time you want the questions to be asked (config.yaml, see the demo)
- You will also need to specify the folder the config file is in, this is done through an enviroment variable called "OTD-Config-Location" (just look up how to set enviroment variables on your specific OS) e.g: "C:\Users\Your-Username-Here\Desktop\config.yaml"
- You will need a discord application to use this, set one up here: https://discord.com/developers/applications
- The current release is only for windows but you should be able to download the source and then build it for your specific OS
- I don't provide a way of hosting but if you want a suggestion I am using AWS EC2 Windows 10 Instance
- If you care enough I am using DSharpPlus to make this (the dlls should be included with the source code)
- If you need any help with anything email me at contact@techyte.net

List of current commands:

- 'remove', removes a question based with the same content as the text you give it, notifys the person that asked the question
- 'quietRemove', same as 'cut' but does not notify the person that asked the question
- 'removeAllBy', removes all the questions asked by the person you provide
- 'forceGen', forces the next question to be the text you give it
- 'forceSpec', forces the next question to be a with the same content as the text you give it, notifys the person that asked the question
- 'clearForce', clears forced question and uses a regular random question
- 'timeDebug', prints out the target time to ask the question and the current time
- 'stop', stops the program and turns off the bot
- 'sendMod', sends a message with the content of what you give it to the mod channel
- 'readout', prints out every current possible question plus who asked it
- 'changeTimeHour', changes the hour that the question will be asked to what you provide (24 hour time)
- 'changeTimeMinute', changes the minute that the question will be asked at
- 'askQuestion', asks the question imedeatly 
- 'resetTime', resets any changes to the time that the question will be asked

List of commands that can be run through Discord:

- '!readout', displays out every current possible question plus who asked it
- '!remove', removed the question with the same content as what you give it, notifys the person that asked it
- '!quietRemove', removes the question with the same content as what you give it without notifying the person that asked it
- '!removeAllBy', removes all question submitted by the user you provide
- '!timeDebug', displays the time that the bot will ask the question
- '!changeTimeHour', changes the hour the bot will ask the question at
- '!changeTimeMinute', changes the minute the bot will ask the question at
- '!resetTime', resets any modifications to the time the bot will ask the question at
- '!stop', stops the bot
- '!askQuestion', forces the bot to ask the question
- '!info', displays information about the bot
- '!commandList', displays the list of possible commands
