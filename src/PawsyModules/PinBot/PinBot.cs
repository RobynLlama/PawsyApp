using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using PawsyApp.PawsyCore;
using PawsyApp.PawsyCore.Modules;

using PinBot.Settings;

namespace PinBot;

[PawsyModule(ModuleName)]
public class PinBot : GuildModule
{
    public const string ModuleName = "pin-bot";
    protected PinBotSettings Settings;

    public PinBot(Guild Owner) : base(Owner, ModuleName, true, true)
    {
        Settings = (this as ISettingsOwner).LoadSettings<PinBotSettings>();
    }

    public override void OnActivate()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            //Add owner.Event callbacks here
        }
    }

    public override void OnDeactivate()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            //Remove owner.Event callbacks here
        }
    }

    public override SlashCommandBundle OnCommandsDeclared(SlashCommandBuilder builder)
    {

        //Add slash commands here

        builder
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("pin")
            .WithDescription("pin a message (by message link)")
            .WithType(ApplicationCommandOptionType.String)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("unpin")
            .WithDescription("unpin a message (by message link)")
            .WithType(ApplicationCommandOptionType.String)
        );

        return new SlashCommandBundle(ModuleCommandHandler, builder.Build(), Name);
    }

    public override void OnConfigDeclared(SlashCommandOptionBuilder rootConfig)
    {

        //add config options to module-manage here

        rootConfig
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Role)
            .WithName("pin-role")
            .WithDescription("Add or remove a role's permission to use pin commands")
        );
    }

    public override Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options)
    {
        var option = options.Options.First();
        var optionName = option.Name;
        var optionValue = option.Value;

        switch (optionName)
        {
            case "pin-role":
                if (optionValue is not ulong roleID)
                    return command.RespondAsync("Somehow that role is invalid");

                if (Settings.RolesWithPerms.ContainsKey(roleID))
                {
                    Settings.RolesWithPerms.Remove(roleID, out var _);
                    command.RespondAsync($"Removed permissions from <@&{roleID}>");
                }
                else
                {
                    Settings.RolesWithPerms.TryAdd(roleID, true);
                    command.RespondAsync($"Added permissions to <@&{roleID}>");
                }

                (Settings as ISettings).Save<PinBotSettings>(this);

                return Task.CompletedTask;
            default:
                return command.RespondAsync("Something went wrong in HandleConfig", ephemeral: true); ;
        }
    }

    protected (ulong ChannelID, ulong MessageID) ParseMessageLink(string link)
    {
        return (0u, 0u);
    }

    protected bool HasPermissions(SocketGuildUser user)
    {
        foreach (var role in user.Roles)
        {
            if (Settings.RolesWithPerms.ContainsKey(role.Id))
                return true;
        }

        return false;
    }

    private Task ModuleCommandHandler(SocketSlashCommand command)
    {

        if (Settings is null)
            return command.RespondAsync("Settings are null in command handler", ephemeral: true);

        if (!Owner.TryGetTarget(out var owner))
            return command.RespondAsync("Unable to locate module owner in command handler", ephemeral: true);

        var options = command.Data.Options.First();
        var link = options.Options.First().Value.ToString();

        if (link is null)
            return command.RespondAsync("Invalid link in command handler");

        var (ChannelID, MessageID) = ParseMessageLink(link);

        if (ChannelID == 0u || MessageID == 0)
            return command.RespondAsync("Unable to resolve that message, sorry", ephemeral: true);

        if (owner.DiscordGuild.GetChannel(ChannelID) is not SocketTextChannel tChannel)
            return command.RespondAsync("Unable to resolve a text channel from that link", ephemeral: true);

        if (tChannel.GetMessageAsync(MessageID).Result is not IUserMessage sMessage || sMessage is null)
            return command.RespondAsync("I'm Unable to find or see that message", ephemeral: true);

        var commandName = options.Name;

        //check user perms
        if (command.User is not SocketGuildUser gUser)
            return command.RespondAsync("Run this command from within a guild, please", ephemeral: true);

        LogAppendContext("PinBot accessed", [
            ("User", gUser.Id),
            ("Name", gUser.DisplayName),
            ("Link", link),
            ("ChannelID", ChannelID),
            ("MessageID", MessageID)
        ]);

        if (!HasPermissions(gUser))
            return command.RespondAsync("You don't have permission to use this command, meow", ephemeral: true);

        switch (commandName)
        {
            case "pin":
                if (sMessage.IsPinned)
                    return command.RespondAsync("This message is already pinned", ephemeral: true);

                try
                {
                    sMessage.PinAsync();
                }
                catch (Exception ex)
                {
                    LogAppendLine($"Encountered an error while pinning {ex}");
                    return command.RespondAsync($"Exception encountered while pinning message {ex.GetType().Name}", ephemeral: true);
                }

                return command.RespondAsync("Done, message pinned", ephemeral: true);
            case "unpin":
                if (!sMessage.IsPinned)
                    return command.RespondAsync("This message is not pinned", ephemeral: true);

                try
                {
                    sMessage.UnpinAsync();
                }
                catch (Exception ex)
                {
                    LogAppendLine($"Encountered an error while pinning {ex}");
                    return command.RespondAsync($"Exception encountered while pinning message {ex.GetType().Name}", ephemeral: true);
                }

                return command.RespondAsync("Done, message pinned", ephemeral: true);
            default:
                return command.RespondAsync("Something went wrong in command handler", ephemeral: true); ;
        }
    }

    // Un-comment this if you need to cleanup before the module is destroyed
    // public override void Destroy() { }
}
