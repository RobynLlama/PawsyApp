using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
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
            owner.OnGuildThreadCreated += ThreadCreated;
        }
    }

    public override void OnDeactivate()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            owner.OnGuildThreadCreated -= ThreadCreated;
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
            .WithDescription("Add or remove a role's permission to use pin commands"))
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Channel)
            .WithName("auto-pin-channel")
            .WithDescription("Add or remove a forum channel to auto pin its first message"));
    }

    public override Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options)
    {
        var option = options.Options.First();
        var optionName = option.Name;
        var optionValue = option.Value;

        switch (optionName)
        {
            case "pin-role":
                if (optionValue is not SocketRole role)
                    return command.RespondAsync("Somehow that role is invalid", ephemeral: true);

                if (Settings.RolesWithPerms.ContainsKey(role.Id))
                {
                    Settings.RolesWithPerms.Remove(role.Id, out var _);
                    command.RespondAsync($"Removed permissions from <@&{role.Id}>");
                }
                else
                {
                    Settings.RolesWithPerms.TryAdd(role.Id, true);
                    command.RespondAsync($"Added permissions to <@&{role.Id}>");
                }

                (Settings as ISettings).Save<PinBotSettings>(this);

                return Task.CompletedTask;
            case "auto-pin-channel":
                if (optionValue is not SocketForumChannel channel)
                    return command.RespondAsync("Only forum channels please");

                if (Settings.AutoPinChannels.ContainsKey(channel.Id))
                {
                    Settings.AutoPinChannels.Remove(channel.Id, out var _);
                    command.RespondAsync($"Removed auto pin from {channel.Mention}");
                }
                else
                {
                    Settings.AutoPinChannels.TryAdd(channel.Id, true);
                    command.RespondAsync($"Added auto pin to {channel.Mention}");
                }

                (Settings as ISettings).Save<PinBotSettings>(this);

                return Task.CompletedTask;
            default:
                return command.RespondAsync("Something went wrong in HandleConfig", ephemeral: true); ;
        }
    }

    protected (ulong ChannelID, ulong MessageID) ParseMessageLink(string link)
    {
        //https://discord.com/channels/1168655651455639582/1169069074572116041/1320527286348415006
        //server: 1168655651455639582
        //channel: 1169069074572116041
        //message: 1320527286348415006
        var mLink = link.Replace("https://discord.com/channels/", string.Empty);

        int sCount = mLink.Count(thing => thing == '/');

        if (sCount != 2)
            return (0u, 0u);

        var values = mLink.Split('/');

        if (!ulong.TryParse(values[1], out var cID))
            return (0u, 0u);

        if (!ulong.TryParse(values[2], out var mID))
            return (0u, 0u);

        return (cID, mID);
    }

    protected bool HasPermissions(SocketGuildUser user, SocketTextChannel channel)
    {
        if (user.GetPermissions(channel).ManageMessages) return true;

        foreach (var role in user.Roles)
        {
            if (Settings.RolesWithPerms.ContainsKey(role.Id))
                return true;
        }

        return false;
    }

    protected bool CanPinMessage(SocketGuildUser user, SocketTextChannel channel)
    {
        if (HasPermissions(user, channel)) return true;

        if (channel is not SocketThreadChannel tChannel)
            return false;

        if (tChannel.Owner.GuildUser != user)
            return false;

        return true;
    }

    private Task ModuleCommandHandler(SocketSlashCommand command)
    {

        if (Settings is null)
            return command.RespondAsync("Settings are null in command handler", ephemeral: true);

        if (!Owner.TryGetTarget(out var owner))
            return command.RespondAsync("Unable to locate module owner in command handler", ephemeral: true);

        var options = command.Data.Options.FirstOrDefault();
        var link = options?.Value.ToString();

        if (link is null)
        {
            return command.RespondAsync("You sent me a null link somehow, meow", ephemeral: true);
        }

        if (options is null)
        {
            LogAppendContext("Null options in CommandHandler for PinBot", [
                ("Triggered by", command.User),
                ("Channel in", command.Channel.Name),
                ("Option count", command.Data.Options.Count)
            ]);

            return command.RespondAsync("An error has occurred while processing your input, options should never be null :[");
        }

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

        if (!CanPinMessage(gUser, tChannel))
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

                return command.RespondAsync("Done, message unpinned", ephemeral: true);
            default:
                return command.RespondAsync("Something went wrong in command handler", ephemeral: true); ;
        }
    }

    private async Task ThreadCreated(SocketThreadChannel channel)
    {
        if (Settings is null)
            return;

        if (!Settings.AutoPinChannels.ContainsKey(channel.ParentChannel.Id))
            return;

        if (channel.ParentChannel is not SocketForumChannel)
            return;

        var message = await channel.GetMessageAsync(channel.Id);

        if (message is not IUserMessage uMessage)
            return;

        if (uMessage.IsPinned)
            return;

        _ = LogAppendContext("PinBot AutoPin accessed", [
            ("ThreadID", channel.Id),
            ("ThreadName", channel.Name),
            ("ParentChannelId", channel.ParentChannel.Id),
            ("ParentChannel", channel.ParentChannel.Name),
            ("UserId", channel.Owner.Id),
            ("User", channel.Owner.DisplayName),
            ("MessageContent", uMessage.CleanContent)
        ]);

        try
        {
            await uMessage.PinAsync();
        }
        catch (Exception ex)
        {
            await LogAppendLine($"Encountered an error while auto pinning {ex}");
        }
    }

    // Un-comment this if you need to cleanup before the module is destroyed
    // public override void Destroy() { }
}
