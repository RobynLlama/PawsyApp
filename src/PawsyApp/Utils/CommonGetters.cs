using Discord.WebSocket;

namespace PawsyApp.Utils;

internal partial class Helpers
{
    internal static SocketGuild? GetGuild(ulong? guildID)
    {
        if (guildID is not ulong realID)
            return null;

        if (PawsyProgram.SocketClient.GetGuild(realID) is SocketGuild guild)
        {
            return guild;
        }

        return null;
    }
}
