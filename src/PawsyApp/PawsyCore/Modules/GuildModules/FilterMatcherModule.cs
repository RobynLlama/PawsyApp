using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Settings;

namespace PawsyApp.PawsyCore.Modules.GuildModules;

internal class FilterMatcherModule : GuildModule
{
    protected FilterMatcherSettings Settings;
    protected ulong LastDeletedMessage = 0;

    public FilterMatcherModule(Guild Owner) : base(Owner, "filter-matcher", true, true)
    {
        Settings = (this as ISettingsOwner).LoadSettings<FilterMatcherSettings>();

        LogAppendContext("Filters loaded", [
            ("Filter Count", Settings.RuleList.Count.ToString())
        ]);
    }

    public override void OnActivate()
    {
        Owner.OnGuildMessage += MessageCallBack;
        Owner.OnGuildMessageEdit += MessageCallBack;
    }

    public override void OnDeactivate()
    {
        Owner.OnGuildMessage -= MessageCallBack;
        Owner.OnGuildMessageEdit -= MessageCallBack;
    }

    public override void OnConfigDeclared(SlashCommandOptionBuilder rootConfig)
    {
        rootConfig
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Channel)
            .WithName("alert-channel")
            .WithDescription("The channel pawsy should use to alert staff")
        );
    }

    public override SlashCommandBundle OnCommandsDeclared(SlashCommandBuilder builder)
    {
        builder
        .WithDefaultMemberPermissions(GuildPermission.ManageGuild)
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommandGroup)
            .WithName("filters")
            .WithDescription("Manage filters")
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithName("add")
                .WithDescription("Add a new filter")
            )
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithName("edit")
                .WithDescription("Edit an existing filter")
            )
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithName("remove")
                .WithDescription("remove an existing filter")
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("rule-id")
                    .WithType(ApplicationCommandOptionType.Integer)
                    .WithDescription("The ID of the rule to remove")
                    .WithMinValue(0)
                )
            )
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithName("list")
                .WithDescription("list existing filters")
            )
        );

        return new SlashCommandBundle(FilterMatcherHandler, builder.Build(), Name);
    }

    private async Task FilterMatcherHandler(SocketSlashCommand command)
    {
        var option = command.Data.Options.First().Options.First();
        var subOpts = option.Options;
        var optionName = option.Name;

        switch (optionName)
        {
            case "add":
            case "edit":
                await command.RespondAsync("Not implemented yet, meow", ephemeral: true);
                return;
            case "remove":
                if (subOpts.First().Value is long ruleID)
                {
                    Settings.RuleList.Remove(ruleID, out _);
                    (Settings as ISettings).Save<FilterMatcherSettings>(this);
                    await command.RespondAsync($"Rule {ruleID} deleted, meow", ephemeral: true);
                    return;
                }
                await command.RespondAsync($"Something went wrong, mew!", ephemeral: true);
                return;
            case "list":
                StringBuilder sb = new("## Rules:\n");
                foreach (var item in Settings.RuleList)
                {
                    sb.Append("- ");
                    sb.Append(item.Key);
                    sb.Append(": ");
                    sb.Append("**");
                    sb.Append(item.Value.RuleName);
                    sb.Append("**");
                    sb.Append(' ');
                    sb.Append('`');
                    sb.Append(item.Value.Regex);
                    sb.AppendLine("`");
                }
                await command.RespondAsync(sb.ToString(), ephemeral: true);
                return;
            default:
                await command.RespondAsync("Something went wrong in FilterMatcherHandler, meow!", ephemeral: true);
                return;
        }
    }

    public override async Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options)
    {
        if (Settings is null)
        {
            await command.RespondAsync("Config is unavailable in HandleConfig", ephemeral: true);
            return;
        }

        var option = options.Options.First();
        var optionName = option.Name;
        var optionValue = option.Value;

        switch (optionName)
        {
            case "alert-channel":
                if (optionValue is not SocketTextChannel optionChannel)
                {
                    await command.RespondAsync("Only text channels, please and thank mew", ephemeral: true);
                    return;
                }

                Settings.LoggingChannelID = optionChannel.Id;
                (Settings as ISettings).Save<FilterMatcherSettings>(this);
                await command.RespondAsync($"Set alert channel to <#{optionChannel.Id}>");
                return;
            default:
                await command.RespondAsync("Something went wrong in HandleConfig", ephemeral: true);
                return;
        }
    }

    private async Task MessageCallBack(SocketUserMessage message, SocketGuildChannel channel)
    {

        if (Settings is null)
            return;

        /*await WriteLog.Cutely("Filter heard a message callback",
        [
            ("ID", message.Id),
            ("Author", message.Author),
            ("Guild", channel.Guild.Name)
        ]);*/

        if (message.Author is SocketGuildUser gUser)
        {
            if (gUser.GetPermissions(channel).ManageMessages && gUser.Id != 156515680353517568)
            {
                await LogAppendLine("User is exempt from filters");
                return;
            }
        }

        if (LastDeletedMessage == message.Id)
        {
            await LogAppendLine("Already deleted this one before..");

            using StreamWriter writer = new("Pawsy.Errors.log", true);
            writer.WriteLine("Pawsy tried to delete a message twice");
            writer.WriteLine(Environment.StackTrace);
            writer.WriteLine();
            writer.Flush();

            return;
        }


        List<Task> tasks = [];

        foreach (var item in Settings.RuleList.Values)
        {
            if (item.Match(message.CleanContent, channel))
            {
                if (item.WarnStaff)
                {
                    if (channel.Guild.GetChannel(Settings.LoggingChannelID) is SocketTextChannel logChannel)
                    {
                        tasks.Add(LogAppendLine("Filter is alerting staff about a message"));
                        tasks.Add(SendMessageReport(logChannel, message, item));
                    }

                }

                //await message.Channel.SendMessageAsync(text: "Oopsie daisy! (✿◠‿◠) Your message got deleted for using naughty words. Pwease keep it pawsitive and kind! Let's keep our chat fun and fwiendly~ ≧◡≦");
                if (item.SendResponse)
                {
                    tasks.Add(LogAppendLine("Filter responding to a message"));
                    tasks.Add(message.Channel.SendMessageAsync(text: item.ResponseMSG));
                }

                if (item.DeleteMessage)
                {
                    LastDeletedMessage = message.Id;
                    tasks.Add(LogAppendLine("Filter is deleting a message"));

                    var m = await message.Channel.GetMessageAsync(message.Id);
                    tasks.Add(m.DeleteAsync());
                }

                break;
            }
        }

        await Task.WhenAll(tasks);
    }

    private static Task<Discord.Rest.RestUserMessage> SendMessageReport(SocketTextChannel channel, SocketMessage message, RuleBundle violation)
    {
        Embed embed = new EmbedBuilder()
        .WithTitle("Detected message")
        .WithDescription($"{message.Author}(<@{message.Author.Id}>)\nMessage ID:{message.Id}\nContents:\n\n{message.CleanContent}")
        .WithColor(violation.ColorR, violation.ColorG, violation.ColorB)
        .WithFooter($"Rule: {violation.RuleName}")
        .WithCurrentTimestamp()
        .Build();
        return channel.SendMessageAsync(embed: embed);
    }
}
