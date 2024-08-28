using PawsyApp.Settings;
using PawsyApp.GuildStorage;
using Discord.WebSocket;

namespace PawsyApp.Utils;

internal class CommonGetters
{
    internal static SocketGuild? GetGuild(ulong? guildID)
    {
        if (guildID is not ulong realID)
            return null;

        if (PawsyProgram._client?.GetGuild(realID) is SocketGuild guild)
        {
            return guild;
        }

        return null;
    }

    internal static GuildSettings? GetSettings(ulong? guildID)
    {
        if (guildID is not ulong realID)
            return null;

        if (AllSettings.GuildSettingsStorage.TryGetValue(realID, out GuildSettings? value))
        {
            return value;
        }

        return null;
    }
}
