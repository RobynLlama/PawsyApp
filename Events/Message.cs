using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.GuildStorage;
using PawsyApp.Utils;

namespace PawsyApp.Events;

internal class MessageEvent
{
    internal static async Task Handler(SocketMessage message)
    {
        if (message.Author.IsBot || message.Author.IsWebhook)
            return;

        if (message.Channel is SocketDMChannel DMchannel)
        {
            WriteLog.Cutely("Pawsy heard a DM!", [
            ("Author", message.Author.GlobalName),
            ("CleanContent", message.CleanContent),
            ("Guild", guild.Name ?? "Unknown"),
            ]);

            if (PawsyProgram.SettingsStorage.TryGetValue(guild.Id, out var settings))
            {

                SocketTextChannel channel = (SocketTextChannel)guild.GetChannel(settings.LoggingChannelID);

                if (channel is not null)
                {
                    foreach (var item in settings.rules)
                    {
                        if (item.Match(message.CleanContent))
                        {
                            if (item.warn_staff)
                                Chirp(channel, message, item);
                            //await message.Channel.SendMessageAsync(text: "Oopsie daisy! (✿◠‿◠) Your message got deleted for using naughty words. Pwease keep it pawsitive and kind! Let's keep our chat fun and fwiendly~ ≧◡≦");
                            if (item.send_response)
                                await message.Channel.SendMessageAsync(text: item.response);

                            if (item.delete_message)
                                await message.DeleteAsync();

                            break;
                        }
                    }


                    static void Chirp(SocketTextChannel channel, SocketMessage message, RuleBundle violation)
                    {
                        Embed embed = new EmbedBuilder().WithAuthor("Pawsy").WithTitle("Detected message").WithDescription($"{message.Author}({message.Author.Id})\n\nLink: <@{message.Author.Id}>\nContents: {message.CleanContent}").WithColor(violation.color_R, violation.color_G, violation.color_B).WithFooter($"Rule: {violation.name}").Build();
                        channel.SendMessageAsync(embed: embed);
                    }
                }
            }
        }

        return;
    }
}
