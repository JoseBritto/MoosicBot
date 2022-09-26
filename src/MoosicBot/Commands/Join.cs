using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using Serilog.Core;
using Victoria;

namespace MoosicBot.Commands;

public class Join  : InteractionModuleBase
{
    private readonly LavaNode _node;
    private readonly DiscordSocketClient _client;

    private readonly ILogger _log;
    public Join(LavaNode node, DiscordSocketClient client)
    {
        _node = node;
        _client = client;

        _log = Log.Logger.ForContext<Join>();
    }


    [SlashCommand("join", "Joins the provided channel or the one you are currently in")]
    [RequireContext(ContextType.Guild)]
    public async Task JoinAsync(IVoiceChannel? channel = null)
    {
        if (channel == null)
        {
            if (Context.User is not IVoiceState voiceState || voiceState?.VoiceChannel == null)
            {
                await FollowupAsync(@"You didn't tell me where to join. ¯\_(ツ)_/¯");
                return;
            }

            channel = voiceState.VoiceChannel;
        }
        
        
        if (channel.GuildId != Context.Guild.Id)
        {
            await FollowupAsync("😥 I can't join voice channels in other servers");
            return;
        }


        if (channel is IStageChannel)
        {
            // TODO: SUPPORT STAGES
            await FollowupAsync("Stages aren't supported yet.");
            return;
        }

        if (Context.User is not SocketGuildUser user)
        {
            await FollowupAsync($"❌ {Context.User.Username} is not a {typeof(SocketGuildUser)}.\n" +
                                $"This is probably a 🐞. Please report this to my developer asap");
            return;
        }

        var userPerms = user.GetPermissions(channel);
        var bot = await Context.Guild.GetCurrentUserAsync().ConfigureAwait(false);
        if (bot == null)
        {
            throw new NullReferenceException("Bot is null somehow.");
            return;
        }
        
        var botPerms = bot.GetPermissions(channel);

        if (userPerms.Connect == false || userPerms.Speak == false)
        {
            await FollowupAsync($"You don't have enough permissions in {channel.Mention} for this!");
            return;
        }

        if (botPerms.Connect == false || botPerms.Speak == false)
        {
            await FollowupAsync($"I don't have permission to join and speak in {channel.Mention}");
        }

        if (_node.HasPlayer(Context.Guild))
        {
            await FollowupAsync($"I'm already connected to {_node.GetPlayer(Context.Guild).VoiceChannel.Mention}. Use /leave first.");
            return;
        }
        try
        {
            await _node.JoinAsync(channel, Context.Channel as ITextChannel);
        }
        catch (Exception e)
        {
            _log.Error(exception: e, "Failed to join {voice_channel} in {server}.", channel, Context.Guild);
            await FollowupAsync($"❌ An error occured while joining {channel.Mention}\nException: {e.Message}");
            return;
        }

        await FollowupAsync($"Joined {channel.Mention}");

    }
    
}