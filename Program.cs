using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace TutorialBot
{
    class Program
    {   
        //variables
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        public CommandService _commands;
        private IServiceProvider _services;


        private SocketGuild guild;

        //log channel info
        private ulong LogChannelID;
        private SocketTextChannel LogChannel;
        
        //run bot connection
        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();

            string json = File.ReadAllText("secrets.json");
            dynamic secrets = JsonConvert.DeserializeObject(json);

            string discordToken = secrets.DISCORD_TOKEN;

            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, discordToken);

            await _client.StartAsync();

            await Task.Delay(-1);

            //now the bot is online
        }

        //client log
        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        //Register Commands Async
        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        // Read Input and get Output ( RECEIVE AND DO SOMETHING )
        public async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            var channel = _client.GetChannel(LogChannelID) as SocketTextChannel;

            //console log with message received and user info
            Console.WriteLine("-------------\nUser:  " + message.Author.Username + " with ID  " + message.Author.Id +
                              "\nWrite:" +
                              "\n" + message.ToString());

            //return (exit and do nothing) if author of message is the bot
            if (message.Author.IsBot) return;

            int argPos = 0;

            //set command ! for COMMAND actions
            if (message.HasStringPrefix("!", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
                if (result.Error.Equals(CommandError.UnmetPrecondition))
                    await message.Channel.SendMessageAsync(result.ErrorReason);
            }

            //I usually transform text input in lower text ( A -> a ) to facilitate the reading of the text
            var text = message.ToString().ToLower();
        }
    }
}
