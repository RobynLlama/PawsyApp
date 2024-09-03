using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Settings;

namespace PawsyApp.PawsyCore.Modules.GuildModules;

internal class ModderRoleCheckerModule : GuildModule
{
    protected ModderRoleCheckerSettings Settings;

    public ModderRoleCheckerModule(Guild Owner) : base(Owner, "modder-role", declaresConfig: true)
    {
        Settings = (this as ISettingsOwner).LoadSettings<ModderRoleCheckerSettings>();
    }

    public override void OnActivate()
    {
        Owner.OnGuildThreadCreated += ThreadCreated;
    }

    public override void OnDeactivate()
    {
        Owner.OnGuildThreadCreated -= ThreadCreated;
    }

    public override void OnConfigDeclared(SlashCommandOptionBuilder rootConfig)
    {
        rootConfig
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Channel)
            .WithName("alert-channel")
            .WithDescription("The channel where Pawsy will alert staff")
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Role)
            .WithName("modding-role")
            .WithDescription("The role Pawsy will look for")
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Channel)
            .WithName("watch-channel")
            .WithDescription("The channel Pawsy should watch for threads in")
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
                    (Settings as ISettings).Save<ModderRoleCheckerSettings>(this);
                    return $"Set alert channel to <#{alertChannel.Id}>";
                case "watch-channel":
                    if (optionValue is not SocketGuildChannel watchChannel)
                    {
                        return "Only text channels, please and thank mew";
                    }

                    Settings.ModdingChannel = watchChannel.Id;
                    (Settings as ISettings).Save<ModderRoleCheckerSettings>(this);
                    return $"Set watch channel to <#{watchChannel.Id}>";
                case "modding-role":
                    if (optionValue is not SocketRole modderRole)
                    {
                        return "Send only role IDs, please and thank mew";
                    }

                    Settings.ModderRoleID = modderRole.Id;
                    (Settings as ISettings).Save<ModderRoleCheckerSettings>(this);
                    return $"Set modder role to <@&{modderRole.Id}>";
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

        await LogAppendContext("Thread created", [
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
