using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using PawsyApp.PawsyCore;
using PawsyApp.PawsyCore.Modules;

using FilterMatcher.Settings;
using System.Text.RegularExpressions;

namespace FilterMatcher;

[PawsyModule(ModuleName)]
public class FilterMatcherModule : GuildModule
{
    public const string ModuleName = "filter-matcher";
    protected FilterMatcherSettings Settings;
    protected ulong LastDeletedMessage = 0;

    public FilterMatcherModule(Guild Owner) : base(Owner, ModuleName, true, true)
    {
        Settings = (this as ISettingsOwner).LoadSettings<FilterMatcherSettings>();

        LogAppendContext("Filters loaded", [
            ("Filter Count", Settings.RuleList.Count.ToString())
        ]);
    }

    public override void OnActivate()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            owner.OnGuildMessage += MessageCallBack;
            owner.OnGuildMessageEdit += MessageCallBack;
        }
    }

    public override void OnDeactivate()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            owner.OnGuildMessage -= MessageCallBack;
            owner.OnGuildMessageEdit -= MessageCallBack;
        }
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
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("name")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithDescription("The name for the rule")
                    .WithRequired(true)
                )
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("regex")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithDescription("The REGEX for the rule")
                    .WithRequired(true)
                )
            )
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithName("edit")
                .WithDescription("Edit an existing filter")
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("rule-id")
                    .WithType(ApplicationCommandOptionType.Integer)
                    .WithDescription("The ID of the rule to edit")
                    .WithMinValue(0)
                    .WithRequired(true)
                )
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("name")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithDescription("The new name for the rule")
                )
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("regex")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithDescription("The new REGEX for the rule")                    
                )
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("channel")
                    .WithType(ApplicationCommandOptionType.Channel)
                    .WithDescription("The new channel for the rule")
                )
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("delete")
                    .WithType(ApplicationCommandOptionType.Boolean)
                    .WithDescription("Should the triggering message be deleted")
                )
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("reply")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithDescription("The reply pawsy will give to the user")
                )
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("warn-staff")
                    .WithType(ApplicationCommandOptionType.Boolean)
                    .WithDescription("Should staff be warned?")
                )
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("warn-color")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithDescription("The color which staff is warned?")
                )
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
                .AddOption(
                    new SlashCommandOptionBuilder()
                    .WithName("rule-id")
                    .WithType(ApplicationCommandOptionType.Integer)
                    .WithDescription("The ID of the rule to list")
                    .WithMinValue(0)
                )
            )
        );

        return new SlashCommandBundle(FilterMatcherHandler, builder.Build(), Name);
    }

    private async Task FilterMatcherHandler(SocketSlashCommand command)
    {
        var option = command.Data.Options.First().Options.First();
        var subOpts = option.Options.ToList();
        var optionName = option.Name;

        switch (optionName)
        {
            case "add":
                if (subOpts[0].Value is string ruleName && subOpts[1].Value is string ruleRegex)
                {
                    Settings.RuleList.TryAdd(Enumerable.Range(0, int.MaxValue)
                                                                .Select(i => (long)i)
                                                                .First(i => !Settings.RuleList.Keys.Contains(i)),
                                                                new(ruleName,
                                                                ruleRegex));
                    (Settings as ISettings).Save<FilterMatcherSettings>(this);
                    await command.RespondAsync($"Rule {ruleName} added, meow", ephemeral: true);
                    return;
                }
                await command.RespondAsync($"Something went wrong, mew!", ephemeral: true);
                return;
            case "edit":
                if (subOpts.First().Value is long ruleID1 && Settings.RuleList.TryGetValue(ruleID1, out RuleBundle? bundle1))
                {
                    foreach (var item in subOpts)
                    {
                        switch (item.Name)
                        {
                            case "name":
                                bundle1.RuleName = (string)item.Value;
                                break;
                            case "regex":
                                bundle1.Regex = (string)item.Value;
                                bundle1.reg = new(bundle1.Regex, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline, new(0, 0, 1));
                                break;
                            case "channel":
                                ulong id = ((SocketTextChannel)item.Value).Id;
                                if (bundle1.FilteredChannels.Contains(id))
                                {
                                    bundle1.FilteredChannels.Remove(id);
                                    break;
                                }
                                bundle1.FilteredChannels.Add(id);
                                break;
                            case "delete":
                                bundle1.DeleteMessage = (bool)item.Value;
                                break;
                            case "reply":
                                if ((string)item.Value == "null") bundle1.SendResponse = false;
                                else bundle1.SendResponse = true;
                                bundle1.ResponseMSG = (string)item.Value;
                                break;
                            case "warn-staff":
                                bundle1.WarnStaff = (bool)item.Value;
                                break;
                            case "warn-color":
                                (int r, int g, int b) = HexToRGB((string)item.Value);
                                if (r == -1)
                                {
                                    await command.RespondAsync($"Pwease input a valid hexadecimal color value :3", ephemeral: true);
                                    return;
                                }
                                bundle1.WarnColorRed = r; 
                                bundle1.WarnColorGreen = g; 
                                bundle1.WarnColorBlue = b;
                                break;
                        }
                    }
                    Settings.RuleList[ruleID1] = bundle1;
                    (Settings as ISettings).Save<FilterMatcherSettings>(this);

                    await command.RespondAsync($"Modified {bundle1.RuleName}, meow!", ephemeral: true);
                    return;
                }
                await command.RespondAsync($"Something went wrong, mew!", ephemeral: true);
                return;
            case "remove":
                if (subOpts.First().Value is long ruleID2)
                {
                    Settings.RuleList.Remove(ruleID2, out _);
                    (Settings as ISettings).Save<FilterMatcherSettings>(this);
                    await command.RespondAsync($"Rule {ruleID2} deleted, meow", ephemeral: true);
                    return;
                }
                await command.RespondAsync($"Something went wrong, mew!", ephemeral: true);
                return;
            case "list":
                StringBuilder sb ;
                if (subOpts.Count >= 1 && subOpts[0].Value is long ruleID3 && Settings.RuleList.TryGetValue(ruleID3, out RuleBundle? bundle2))
                {
                    sb = new($"## Rule: {bundle2.RuleName}\n");
                    sb.Append($"Regex: ");
                    sb.AppendLine(bundle2.Regex);
                    sb.Append($"Channels: ");
                    bundle2.FilteredChannels.ForEach(channel => sb.Append($"<#{channel}>, "));
                    sb.AppendLine();
                    sb.Append("Reply: ");
                    sb.AppendLine(bundle2.SendResponse ? bundle2.ResponseMSG : "None");
                    sb.Append("Delete Message: ");
                    sb.AppendLine(bundle2.DeleteMessage.ToString());
                    sb.Append("Warn Staff: ");
                    sb.AppendLine(bundle2.WarnStaff.ToString());
                    sb.Append("Warn Color: ");
                    sb.AppendLine($"R: {bundle2.WarnColorRed}, G: {bundle2.WarnColorGreen}, B: {bundle2.WarnColorBlue},");

                    await command.RespondAsync(sb.ToString(), ephemeral: true);
                    return;
                }
                sb = new("## Rules:\n");
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
            if (/*gUser.GetPermissions(channel).ManageMessages && gUser.Id != 156515680353517568*/false)
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
        .WithDescription($"{message.Author}(<@{message.Author.Id}>)\nMessage ID:{message.Id}\nChannel: <#{message.Channel.Id}>\nContents:\n\n{message.CleanContent}")
        .WithColor(violation.WarnColorRed, violation.WarnColorGreen, violation.WarnColorBlue)
        .WithFooter($"Rule: {violation.RuleName}")
        .WithCurrentTimestamp()
        .Build();
        return channel.SendMessageAsync(embed: embed);
    }

    public static (int r, int g, int b) HexToRGB(string hex)
    {
        if (hex.StartsWith("#"))
            hex = hex.Substring(1);

        if (hex.Length != 6)
            return (-1,-1,-1);

        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);

        return (r, g, b);
    }
}
