const { Client, GatewayIntentBits, Partials, } = require('discord.js');
const { token } = require('./config.json');

let messageContents = []

const client = new Client({partials: [Partials.Channel, Partials.Message], intents: [GatewayIntentBits.Guilds, GatewayIntentBits.GuildMessages, GatewayIntentBits.DirectMessages, GatewayIntentBits.GuildMembers, GatewayIntentBits.MessageContent, GatewayIntentBits.DirectMessageReactions] });

client.once('ready', () => {
    console.log('Ready!');

    GetQuestions();
});

client.on('messageCreate', message => {
    if(message.author.bot || message.inGuild()) return;
    message.react('👍');
    questions.push(message);
});

function GetQuestions(){
    const guild = client.guilds.cache.get('954307870869028906');

    guild.members.fetch().then(members =>
    {
        members.forEach(member =>
        {
            if(!member.user.bot){
                guild.members.cache.get(member.id).createDM().then(dmchannel => {
                    var channelID = dmchannel.id;
                    const channel = client.channels.cache.get(channelID);
                    channel.messages.fetch({ limit: 100 }).then(messages => {
                        messages.forEach(message => {
                            if(!message.author.bot){
                                let notTickReactions = 0;
                                message.reactions.cache.forEach(reaction => {
                                    if(reaction.emoji !== '✔'){
                                        notTickReactions++;
                                    }
                                });
                                if(notTickReactions===message.reactions.cache.size){
                                    messageContents.push(message);
                                }
                            }
                        })
                    });
                });
            }
        });
    });
}

client.login(token);

function SendQuestion(){
    client.channels.cache.get("1015459987000147998").fetch().then(fullChannel =>{
        const randomId = Math.floor(Math.random() * messageContents.length);
        fullChannel.send(messageContents[randomId].content);

        messageContents[randomId].react('✔');
    });
}

setInterval(function(){
    var date = new Date();
    if(date.getHours() === 13 && date.getMinutes() === 0){
        SendQuestion();
    }
}, 60000);
