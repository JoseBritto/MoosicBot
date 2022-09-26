using Config.Net;
using MoosicBot;


const string CONFIG_FILE = "config.json";

var settings = new ConfigurationBuilder<ISettings>()
    .UseJsonFile(CONFIG_FILE)
    .UseEnvironmentVariables()
    .Build();

Console.WriteLine("The token is: ");
Console.WriteLine(settings.DiscordToken);

// to be done