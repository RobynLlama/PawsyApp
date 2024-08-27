using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.GuildStorage;
using PawsyApp.Settings;
using PawsyApp.Utils;

namespace PawsyApp.Events;

internal class MessageEvent
{
    internal static Emote PawsySmall = new(1277935719805096066, "pawsysmall");
    internal static async Task Respond(SocketMessage message)
    {
        if (message.Author.IsBot || message.Author.IsWebhook || message.Source == MessageSource.System)
            return;

        var tasks = new List<Task>();

        if (message.Channel is SocketDMChannel DMchannel)
        {
            WriteLog.Cutely("Pawsy heard a DM!", [
            ("Author", message.Author.GlobalName),
            ("CleanContent", message.CleanContent),
            ]);

            await DMchannel.SendMessageAsync("Hi! I'm Pawsy <:pawsysmall:1277935719805096066> the cutie kitty app here to keep you safe and sound! Sorry, but I'm really shy in DMs ≧◡≦ please just talk to me on the server, okay?");
            return;
        }

        if (message.Channel is not SocketGuildChannel guildChannel)
            return;

        var guild = guildChannel.Guild;
        var AuthorName = message.Author.GlobalName ?? message.Author.Username;

        if (message.CleanContent.Contains("pawsy", System.StringComparison.InvariantCultureIgnoreCase))
        {
            tasks.Add(message.AddReactionAsync(PawsySmall));
        }

        WriteLog.Cutely("Pawsy heard this!", [
            ("Author", AuthorName),
            ("CleanContent", message.CleanContent),
            ("Channel", guildChannel.Name),
            ("Guild", guild.Name ?? "Unknown"),
            ]);

        if (!AllSettings.GuildSettingsStorage.TryGetValue(guild.Id, out var settings))
        {
            await Task.WhenAll(tasks);
            return;
        }


        if (guild.GetChannel(settings.LoggingChannelID) is not SocketTextChannel channel)
        {
            await Task.WhenAll(tasks);
            return;
        }

        foreach (var item in settings.rules)
        {
            if (item.Match(message.CleanContent))
            {
                if (item.warn_staff)
                    tasks.Add(Chirp(channel, message, item));
                //await message.Channel.SendMessageAsync(text: "Oopsie daisy! (✿◠‿◠) Your message got deleted for using naughty words. Pwease keep it pawsitive and kind! Let's keep our chat fun and fwiendly~ ≧◡≦");
                if (item.send_response)
                    tasks.Add(message.Channel.SendMessageAsync(text: item.response));

                if (item.delete_message)
                    tasks.Add(message.DeleteAsync());

                break;
            }
        }

        static Task Chirp(SocketTextChannel channel, SocketMessage message, RuleBundle violation)
        {
            Embed embed = new EmbedBuilder().WithAuthor("Pawsy").WithTitle("Detected message").WithDescription($"{message.Author}({message.Author.Id})\n\nLink: <@{message.Author.Id}>\nContents: {message.CleanContent}").WithColor(violation.color_R, violation.color_G, violation.color_B).WithFooter($"Rule: {violation.name}").Build();
            return channel.SendMessageAsync(embed: embed);
        }

        await Task.WhenAll(tasks);
        return;
    }
}
