using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Victoria;

namespace MoosicBot;

public class Bot
{
    private readonly ISettings _settings;

    private readonly ILogger _log;
    private readonly ILogger _discordLog;
    private readonly ILogger _lavaLog;
    private DiscordSocketClient client;
    private ServiceProvider provider;

    public Bot(ISettings settings)
    {
        _settings = settings;
        _log = Log.Logger.ForContext<Bot>();
        _discordLog = Log.Logger.ForContext("SourceContext", "Discord");
        _lavaLog = Log.Logger.ForContext("SourceContext", "Lavalink");
    }


    public async Task StartAsync()
    {
        _log.Information("Connecting to discord...");

        provider = ConfigureServices(new ServiceCollection());

        await provider.GetRequiredService<InteractionService>()
            .AddModulesAsync( Assembly.GetAssembly(typeof(Bot)), provider);
        
        client = provider.GetRequiredService<DiscordSocketClient>();

        client.Log += DiscordLog;
        client.Ready += ClientReady;
        client.InteractionCreated += InteractionCreated;
        client.MessageReceived += ClientMessageReceived;

        provider.GetRequiredService<InteractionService>().SlashCommandExecuted += PostCommandHandle;
        
        provider.GetRequiredService<LavaNode>().OnLog += LavaLog;
       
        await client.LoginAsync(TokenType.Bot, _settings.DiscordToken);

        await client.StartAsync();

       
    }

    private async Task PostCommandHandle(SlashCommandInfo info, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess) return;

        switch (result.Error)
        {
            case InteractionCommandError.UnknownCommand:
                break;
            case InteractionCommandError.ConvertFailed:
                break;
            case InteractionCommandError.BadArgs:
                break;
            case InteractionCommandError.Exception:
                await context.Interaction.FollowupAsync($"🤕 An exception occured: {result.ErrorReason}");
                break;
            case InteractionCommandError.Unsuccessful:
                break;
            case InteractionCommandError.UnmetPrecondition:
                break;
            case InteractionCommandError.ParseFailed:
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private Task ClientMessageReceived(SocketMessage message)
    {
        if(_settings.AdminIds.Contains(message.Author.Id.ToString()) == false) return Task.CompletedTask;
        
        if(message.MentionedUsers.Any(x => x.Id == client.CurrentUser.Id) == false) return Task.CompletedTask;
        
        if (message.Content.Contains("refresh guild"))
        {
            if (message.Channel is not SocketGuildChannel channel) return Task.CompletedTask;

            var id = channel.Guild.Id;
            provider.GetRequiredService<InteractionService>().RegisterCommandsToGuildAsync(id);
            _log.Information("Refreshing commands for {guild} on request by {user}", channel.Guild, message.Author);
            return Task.CompletedTask;
        }

        if (message.Content.Contains("refresh global"))
        {
            _log.Information("Refreshing commands globally on request by {user}", message.Author);
            provider.GetRequiredService<InteractionService>().RegisterCommandsGloballyAsync();
        }

        return Task.CompletedTask;
    }

    private Task InteractionCreated(SocketInteraction interaction)
    {
        Task.Run(() => interaction.DeferAsync().Wait());

        var context = new InteractionContext(client, interaction, interaction.Channel);

        Task.Run(() =>
            provider.GetRequiredService<InteractionService>().ExecuteCommandAsync(context, provider)
                .Wait());

        return Task.CompletedTask;
    }

    private Task ClientReady()
    {
        var node = provider.GetRequiredService<LavaNode>();
        
        if (node.IsConnected == false)
            node.ConnectAsync();
        
        return Task.CompletedTask;
    }
    

    private Task DiscordLog(LogMessage log)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        _discordLog.Write((LogEventLevel)(5 - (int)log.Severity), log.Exception, log.Message);
        
        return Task.CompletedTask;
    }

    private Task LavaLog(LogMessage log)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        _lavaLog.Write((LogEventLevel)(5 - (int)log.Severity), log.Exception, log.Message);
        
        return Task.CompletedTask;
    }

    
    private ServiceProvider ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
            }))
            .AddSingleton(new InteractionServiceConfig
            {
                LogLevel = LogSeverity.Info,
                DefaultRunMode = RunMode.Async,
            })
            .AddSingleton<InteractionService>()
            .AddLavaNode(x =>
            {
                x.SelfDeaf = true;
                x.Hostname = _settings.Lavalink.HostName;
                x.Authorization = _settings.Lavalink.Password;
                x.Port = _settings.Lavalink.Port;
                x.EnableResume = true;
                x.IsSsl = _settings.Lavalink.UseSSl;
                x.LogSeverity = LogSeverity.Info;
                
            });

        return serviceCollection.BuildServiceProvider();
    }
}