using Discord.Interactions;
using Discord.WebSocket;

namespace MoosicBot.Commands;

public class Ping : InteractionModuleBase
{
    private readonly DiscordSocketClient _client;

    public Ping(DiscordSocketClient client)
    {
        _client = client;
    }


    [SlashCommand("ping", "Sends pong to you")]
    public async Task PingAsync()
    {
        await FollowupAsync($"🏓 Pong! That was {_client.Latency}ms");
    }
}