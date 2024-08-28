using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace PawsyApp.Events;

internal class MessageUpdatedEvent
{
    internal static async Task Respond(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel channel) => await MessageEvent.Respond(message);
}
