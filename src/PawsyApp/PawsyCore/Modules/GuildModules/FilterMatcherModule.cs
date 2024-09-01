using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules.GuildModules;

internal class FilterMatcherModule : GuildModule
{
    protected FilterMatcherSettings Settings;
    protected ulong LastDeletedMessage = 0;

    public FilterMatcherModule(Guild Owner) : base(Owner, "filter-matcher")
    {
        Settings = (this as ISettingsOwner).LoadSettings<FilterMatcherSettings>();

        WriteLog.Cutely("Filters loaded", [
            ("Filter Count", Settings.RuleList.Count.ToString())
        ]);
    }

    public override void OnModuleActivation()
    {
        Owner.OnGuildMessage += MessageCallBack;
        Owner.OnGuildMessageEdit += MessageCallBack;
    }

    public override void OnModuleDeactivation()
    {
        Owner.OnGuildMessage -= MessageCallBack;
        Owner.OnGuildMessageEdit -= MessageCallBack;
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
                await WriteLog.LineNormal("User is exempt from filters");
                return;
            }
        }

        if (LastDeletedMessage == message.Id)
        {
            await WriteLog.LineNormal("Already deleted this one before..");

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
                        tasks.Add(WriteLog.LineNormal("Filter is alerting staff about a message"));
                        tasks.Add(SendMessageReport(logChannel, message, item));
                    }

                }

                //await message.Channel.SendMessageAsync(text: "Oopsie daisy! (✿◠‿◠) Your message got deleted for using naughty words. Pwease keep it pawsitive and kind! Let's keep our chat fun and fwiendly~ ≧◡≦");
                if (item.SendResponse)
                {
                    tasks.Add(WriteLog.LineNormal("Filter responding to a message"));
                    tasks.Add(message.Channel.SendMessageAsync(text: item.ResponseMSG));
                }

                if (item.DeleteMessage)
                {
                    LastDeletedMessage = message.Id;
                    tasks.Add(WriteLog.LineNormal("Filter is deleting a message"));

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
        .WithDescription($"{message.Author}({message.Author.Id})\n\nLink: <@{message.Author.Id}>\nContents: {message.CleanContent}")
        .WithColor(violation.ColorR, violation.ColorG, violation.ColorB)
        .WithFooter($"Rule: {violation.RuleName}")
        .WithCurrentTimestamp()
        .Build();
        return channel.SendMessageAsync(embed: embed);
    }
}
