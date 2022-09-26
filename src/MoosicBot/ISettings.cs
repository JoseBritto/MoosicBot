using Config.Net;

namespace MoosicBot;

public interface ISettings
{
    [Option(Alias = "discord_token")]
    public string DiscordToken { get; }
    
}