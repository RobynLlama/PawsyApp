using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Core;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

internal class FilterMatcherModule : GuildSubmodule
{
    public override string Name => "filter-matcher";
    public override IModuleSettings? Settings => _settings;

    protected FilterMatcherSettings? _settings;
    protected ulong LastDeletedMessage = 0;

    public override void Alive()
    {
        _settings = (this as IModule).LoadSettings<FilterMatcherSettings>();
        WriteLog.Cutely("Filters loaded", [
            ("Filter Count", _settings.RuleList.Count.ToString())
        ]);
    }

    public override void OnModuleActivation()
    {
        if (Owner is GuildModule guild)
        {
            guild.OnGuildMessage += MessageCallBack;
            guild.OnGuildMessageEdit += MessageCallBack;
        }
    }

    public override void OnModuleDeactivation()
    {
        if (Owner is GuildModule guild)
        {
            guild.OnGuildMessage -= MessageCallBack;
            guild.OnGuildMessageEdit -= MessageCallBack;
        }
    }

    private async Task MessageCallBack(SocketUserMessage message, SocketGuildChannel channel)
    {

        if (_settings is null)
            return;

        /*await WriteLog.Cutely("Filter heard a message callback",
        [
            ("ID", message.Id),
            ("Author", message.Author),
            ("Guild", channel.Guild.Name)
        ]);*/

        if (message.Author is SocketGuildUser gUser)
        {
            if (gUser.GetPermissions(channel).ManageMessages)
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

        foreach (var item in _settings.RuleList.Values)
        {
            if (item.Match(message.CleanContent, channel))
            {
                if (item.WarnStaff)
                {
                    if (channel.Guild.GetChannel(_settings.LoggingChannelID) is SocketTextChannel logChannel)
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
