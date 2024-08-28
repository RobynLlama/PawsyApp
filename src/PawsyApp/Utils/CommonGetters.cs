using PawsyApp.GuildStorage;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules;

namespace PawsyApp.Utils;

internal class CommonGetters
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

    internal static GuildSettings? GetSettings(ulong? guildID)
    {
        if (guildID is not ulong realID)
        {
            WriteLog.Normally("Bad ulong in GetSettings");
            return null;
        }

        if ((PawsyProgram.Pawsy as IModuleIdent).GetModuleIdent<GuildModule>(realID) is not GuildModule mod)
        {
            WriteLog.Normally("failed to acquire module in GetSettings");
            return null;
        }

        if (mod.Settings is GuildSettings settings)
        {
            return settings;
        }

        WriteLog.Normally("failed to get settings from module in GetSettings");
        return null;
    }
}
