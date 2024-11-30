using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using PawsyApp.PawsyCore;
using PawsyApp.PawsyCore.Modules;

using ForumRoleChecker.Settings;
using System.Linq;

namespace ForumRoleChecker;

[PawsyModule(ModuleName)]
public class ForumRoleCheckerModule : GuildModule
{
    public const string ModuleName = "forum-role-checker";
    protected ForumRoleCheckerSettings Settings;

    public ForumRoleCheckerModule(Guild Owner) : base(Owner, ModuleName, declaresConfig: true, declaresCommands: true)
    {
        Settings = (this as ISettingsOwner).LoadSettings<ForumRoleCheckerSettings>();
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
        builder
        .WithDefaultMemberPermissions(GuildPermission.ManageMessages)
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("add-watch-channel")
            .WithDescription("Pawsy will watch a specific forum channel for a specific role")
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Channel)
                .WithName("watched-channel")
                .WithRequired(true)
                .WithDescription("The channel Pawsy will watch")
            )
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Role)
                .WithName("watched-role")
                .WithRequired(true)
                .WithDescription("The channel Role Pawsy will look for")
            )
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("remove-watch-channel")
            .WithDescription("Pawsy will stop watching a specific forum channel")
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Channel)
                .WithName("watched-channel")
                .WithRequired(true)
                .WithDescription("The channel Pawsy will stop watching")
            )
        );

        return new SlashCommandBundle(ForumCheckerHandler, builder.Build(), Name);
    }

    private async Task ForumCheckerHandler(SocketSlashCommand command)
    {
        var name = command.Data.Options.First().Name;
        var options = command.Data.Options.First().Options.ToArray();

        switch (name)
        {
            case "add-watch-channel":
                if (options[0].Value is not SocketForumChannel fChannel)
                {
                    await command.RespondAsync("Only send forum channels, please", ephemeral: true);
                    return;
                }
                if (options[1].Value is not SocketRole role)
                {
                    await command.RespondAsync("Send me a valid role, please", ephemeral: true);
                    return;
                }
                if (Settings.WatchList.TryGetValue(fChannel.Id, out var channel))
                {
                    await command.RespondAsync("This channel already has a watch, mew..", ephemeral: true);
                    return;
                }

                Settings.WatchList[fChannel.Id] = role.Id;
                (Settings as ISettings).Save<ForumRoleCheckerSettings>(this);

                await command.RespondAsync($"Added channel <#{fChannel.Id}> watching for role <@&{role.Id}>");

                return;

            case "remove-watch-channel":
                if (options[0].Value is not SocketForumChannel rChannel)
                {
                    await command.RespondAsync("Only send forum channels, please", ephemeral: true);
                    return;
                }
                if (!Settings.WatchList.TryGetValue(rChannel.Id, out var dChannel))
                {
                    await command.RespondAsync("This channel doesn't have a watch, mew..", ephemeral: true);
                    return;
                }

                if (Settings.WatchList.Remove(rChannel.Id, out var oldRole))
                {
                    await command.RespondAsync($"No longer watching <#{rChannel.Id}> for <@&{oldRole}>");
                    (Settings as ISettings).Save<ForumRoleCheckerModule>(this);
                    return;
                }

                await command.RespondAsync("I was unable to remove that channel, sorry, mew.", ephemeral: true);

                return;
        }

        await command.RespondAsync("Something went wrong, mew..", ephemeral: true);
    }

    public override void OnConfigDeclared(SlashCommandOptionBuilder rootConfig)
    {
        rootConfig
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Channel)
            .WithName("alert-channel")
            .WithDescription("The channel where Pawsy will alert staff")
        );
    }

    public override Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options)
    {

        List<string> output = [];

        foreach (var item in options.Options)
        {
            output.Add($"{item.Name} : {SetOption(item.Name, item.Value)}");
        }

        string SetOption(string optionName, object optionValue)
        {
            switch (optionName)
            {
                case "alert-channel":
                    if (optionValue is not SocketTextChannel alertChannel)
                    {
                        return "Only text channels, please and thank mew";
                    }

                    Settings.AlertChannel = alertChannel.Id;
                    (Settings as ISettings).Save<ForumRoleCheckerSettings>(this);
                    return $"Set alert channel to <#{alertChannel.Id}>";
                default:
                    return "Something went wrong in HandleConfig";
            }
        }

        StringBuilder sb = new("## Settings Changed:");

        foreach (var item in output)
        {
            sb.Append('\n');
            sb.Append("- ");
            sb.Append(item);
        }

        return command.RespondAsync(sb.ToString());
    }

    private async Task ThreadCreated(SocketThreadChannel channel)
    {
        if (Settings is null)
            return;

        await LogAppendLine("Checking if owner needs role");

        if (channel.Owner is null)
        {
            await LogAppendLine("Somehow channel owner is null, aborting");
            return;
        }

        bool ownerNeedsRole = false;

        if (Settings.WatchList.TryGetValue(channel.ParentChannel.Id, out var neededRole))
        {
            var ownerRoles = channel.Guild.GetUser(channel.Owner.Id).Roles;
            ownerNeedsRole = true;

            if (ownerRoles is null)
            {
                await LogAppendLine("Somehow owner roles are null, aborting");
                return;
            }

            foreach (var item in ownerRoles)
            {
                if (item.Id == neededRole)
                {
                    ownerNeedsRole = false;
                    break;
                }
            }
        }

        await LogAppendContext("Thread created", [
                ("Owner", channel.Owner.DisplayName),
                ("Needs Role", ownerNeedsRole),
                ]);

        if (ownerNeedsRole && channel.Guild.GetChannel(Settings.AlertChannel) is SocketTextChannel logChannel)
        {
            Embed embed = new EmbedBuilder()
            .WithTitle("User Needs Role!")
            .WithDescription($"User: <@{channel.Owner.Id}>\nNeeded Role: <@&{neededRole}>\nLink: <#{channel.Id}>")
            .WithColor(32, 16, 128)
            .WithFooter("Forum Role Checker Module")
            .WithCurrentTimestamp()
            .Build();

            await logChannel.SendMessageAsync(embed: embed);
        }
    }
}
