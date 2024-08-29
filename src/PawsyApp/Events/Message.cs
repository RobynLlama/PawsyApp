using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules;
using PawsyApp.PawsyCore.Modules.Core;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.Events;

internal class MessageEvent
{
    internal static Task Respond(SocketMessage message)
    {

        //Filter out bots, system and webhook message
        if (message.Author.IsBot || message.Author.IsWebhook || message.Source == MessageSource.System)
            return Task.CompletedTask;

        WriteLog.Cutely("Pawsy heard this!", [
            ("Author", (message.Author as SocketGuildUser)?.Nickname ?? message.Author.GlobalName ?? message.Author.Username ?? "Nobody"),
            ("CleanContent", message.CleanContent),
            ("Channel", message.Channel.Name),
            ("Guild", (message.Channel as SocketGuildChannel)?.Guild.Name ?? "No Guild"),
            ]);

        return Task.CompletedTask;
    }
}
