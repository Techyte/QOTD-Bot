const { Client, GatewayIntentBits, Partials, } = require('discord.js');
const { token, serverId, channelId, modId } = require('./config.json');

let messageContents = []
let messageContentsIndex = [];

const client = new Client({partials: [Partials.Channel, Partials.Message], intents: [GatewayIntentBits.Guilds, GatewayIntentBits.GuildMessages, GatewayIntentBits.DirectMessages, GatewayIntentBits.GuildMembers, GatewayIntentBits.MessageContent, GatewayIntentBits.DirectMessageReactions] });

client.once('ready', () => {
    console.log('Ready!');

    GetQuestions();
});

client.on('messageCreate', message => {
    if(message.author.bot || message.inGuild()) return;

    if(message.author.id === modId){
        if(message.content.startsWith("!stop")){
            client.destroy();
            return;
        }
    }

    message.react('👍');
    messageContents.push(message);
    messageContentsIndex.push(message.id);
});

client.on('messageDelete', message => {
    if(message.author.bot || message.inGuild()) return;

    for (let i = 0; i < messageContentsIndex.length; i++){
        if (messageContentsIndex[i] === message.id){
            messageContents.splice(i);
            console.log('Someone deleted a message')
        }
    }
})

function GetQuestions(){
    const guild = client.guilds.cache.get(serverId);

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
                                    if(reaction.emoji.toString() !== '✔'){
                                        notTickReactions++;
                                    }
                                });
                                if(notTickReactions===message.reactions.cache.size && message.reactions.cache.size === 0){
                                    messageContents.push(message);
                                    messageContentsIndex.push(message.id);
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
    client.channels.cache.get(channelId).fetch().then(fullChannel =>{
        const randomId = Math.floor(Math.random() * messageContents.length);

        fullChannel.send(messageContents[randomId].content);

        messageContents[randomId].reply("This question was asked! See it here: " + fullChannel.url);

        messageContents[randomId].react('✔');
        messageContentsIndex.splice(randomId)
        messageContents.splice(randomId)
    });
}

setInterval(function() {
    let date = new Date();
    if(date.getHours() === 12 && date.getMinutes() === 0){
        SendQuestion();
    }
}, 60000);
