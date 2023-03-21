<div align="center">
  <a href="https://github.com/RiptideNetworking/Riptide">
    <img src="QOTD_logo.png" width="150px" height="auto">
  </a>
</div>
<div align="center"></div>
<h1 align="center">QOTD Discord Bot</h1>
 
 This readme is up to date as of QOTD v1.4.1
 
## Summary

- This is distrubuted under the MIT Licence
- You DM the bot to give it a question
- Delete a message to remove it from the bots list of questions
- Fully modular command system (operated from the console)
- You need to set up the config file with stuff like the bot token, server id, channel id, etc. (config.yaml, see the demo), this config file must be located in the same location as the exe that runs the bot
- For command privleges and how to set them in the config file you should look at this page on [DSharpPlus Discord Permissions](https://dsharpplus.github.io/api/DSharpPlus.Permissions.html?q=Permissions.Administrator)
- You will need a discord application to use this, set one up here: [Discord Developer Portal](https://discord.com/developers/applications)
- The current release is only for windows but you should be able to download the source and then build it for your specific OS
- I don't provide a way of hosting but if you want a suggestion I am using AWS EC2 Windows 10 Instance
- I am using DSharpPlus to make this (the dlls should be included with the source code)
- If you need any help with anything email me at contact@techyte.net

## List of current commands:

- 'remove', removes a question with the same content as the text you give it, notifys the person that asked the question
- 'quietRemove', same as 'remove' but does not notify the person that asked the question
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

## List of commands that can be run through Discord:

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
