using Config.Net;
using MoosicBot;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract


const string CONFIG_FILE = "config.json";

var settings = new ConfigurationBuilder<ISettings>()
    .UseJsonFile(CONFIG_FILE)
    .UseEnvironmentVariables()
    .Build();


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(LogEventLevel.Debug)
    .WriteTo.Console(
        outputTemplate: "[{SourceContext}({Timestamp:t})] {Level:u4}: {Message:lj}{NewLine}{Exception}"
    )
    .Enrich.FromLogContext()
    .CreateLogger();

Log.Information("Application Started");

try
{
    var bot = new Bot(settings);
    bot.StartAsync().Wait();
    Task.Delay(-1).Wait();
}
catch (Exception e)
{
    Log.Fatal(exception: e, messageTemplate: "Unknown Error Occured!");
}
finally
{
    if(Log.Logger is not null)
        Log.CloseAndFlush();
}

