using Config.Net;

namespace MoosicBot;

public interface ISettings
{
    [Option(Alias = "discord_token")]
    public string DiscordToken { get; }
    
    [Option(Alias = "admin_ids")]
    public string[] AdminIds { get; }

    
    [Option(Alias = "lavalink")]
    public ILavalink Lavalink { get; }
    
    public interface ILavalink
    {
        [Option(Alias = "host_name", DefaultValue = "localhost")]
        public string HostName { get; }
        
        [Option(Alias = "password", DefaultValue = "youshallnotpass")]
        public string Password { get; }
        
        [Option(Alias = "port", DefaultValue = (ushort)2333)]
        public ushort Port { get; }
        
        [Option(Alias = "use_ssl", DefaultValue = false)]
        public bool UseSSl { get; }
    }
}