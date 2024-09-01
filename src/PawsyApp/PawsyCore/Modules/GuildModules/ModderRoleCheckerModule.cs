using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules.GuildModules;

internal class ModderRoleCheckerModule : GuildModule
{
    protected ModderRoleCheckerSettings Settings;

    public ModderRoleCheckerModule(Guild Owner) : base(Owner, "modder-role")
    {
        Settings = (this as ISettingsOwner).LoadSettings<ModderRoleCheckerSettings>();
    }

    public override void OnModuleActivation()
    {
        Owner.OnGuildThreadCreated += ThreadCreated;
    }

    public override void OnModuleDeactivation()
    {
        Owner.OnGuildThreadCreated -= ThreadCreated;
    }

    private async Task ThreadCreated(SocketThreadChannel channel)
    {
        if (Settings is null)
            return;

        if (channel.ParentChannel.Id != Settings.ModdingChannel)
            return;

        var ownerRoles = channel.Guild.GetUser(channel.Owner.Id).Roles;
        bool ownerNeedsRole = true;

        foreach (var item in ownerRoles)
        {
            if (item.Id == Settings.ModderRoleID)
            {
                ownerNeedsRole = false;
                break;
            }
        }

        await WriteLog.Cutely("Thread created", [
                ("Owner", channel.Owner.DisplayName),
                ("Needs Role", ownerNeedsRole),
                ]);

        if (ownerNeedsRole && channel.Guild.GetChannel(Settings.AlertChannel) is SocketTextChannel logChannel)
        {
            Embed embed = new EmbedBuilder()
            .WithTitle("New modders in YOUR area")
            .WithDescription($"A user has posted a mod release thread and doesn't have the modder role\nUser: <@{channel.Owner.Id}>\nLink: <#{channel.Id}>")
            .WithColor(32, 16, 128)
            .WithFooter("Modder Role Checker Module")
            .WithCurrentTimestamp()
            .Build();

            await logChannel.SendMessageAsync(embed: embed);
        }
    }
}
