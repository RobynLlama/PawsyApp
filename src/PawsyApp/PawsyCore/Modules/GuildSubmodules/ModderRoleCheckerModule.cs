using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Core;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

internal class ModderRoleCheckerModule : GuildSubmodule
{
    public override IModule? Owner { get => _owner; set => _owner = value; }
    public override string Name => "ModderRole";
    public override ConcurrentBag<IModule> Modules => _modules;
    public override IModuleSettings? Settings => _settings;

    protected IModule? _owner;
    protected readonly ConcurrentBag<IModule> _modules = [];
    protected ModderRoleCheckerSettings? _settings;

    public override void Alive()
    {
        _settings = (this as IModule).LoadSettings<ModderRoleCheckerSettings>();
    }

    public override void OnModuleActivation()
    {
        if (_owner is GuildModule guild)
        {
            guild.OnGuildThreadCreated += ThreadCreated;
        }
    }

    public override void OnModuleDeactivation()
    {
        if (_owner is GuildModule guild)
        {
            guild.OnGuildThreadCreated -= ThreadCreated;
        }
    }

    private async Task ThreadCreated(SocketThreadChannel channel)
    {
        if (_settings is null)
            return;

        if (channel.ParentChannel.Id != _settings.ModdingChannel)
            return;

        var ownerRoles = channel.Guild.GetUser(channel.Owner.Id).Roles;
        bool ownerNeedsRole = true;

        foreach (var item in ownerRoles)
        {
            if (item.Id == _settings.ModderRoleID)
            {
                ownerNeedsRole = false;
                break;
            }
        }

        await WriteLog.Cutely("Thread created", [
                ("Owner", channel.Owner.DisplayName),
                ("Needs Role", ownerNeedsRole),
                ]);

        if (ownerNeedsRole && channel.Guild.GetChannel(_settings.AlertChannel) is SocketTextChannel logChannel)
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
