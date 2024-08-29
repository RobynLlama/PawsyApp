using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Core;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

internal class FilterMatcherModule : GuildSubmodule
{
    public override IModule? Owner { get => _owner; set => _owner = value; }
    public override string Name => "FilterMatcher";
    public override ConcurrentBag<IModule> Modules => _modules;
    public override IModuleSettings? Settings => _settings;

    protected IModule? _owner;
    protected readonly ConcurrentBag<IModule> _modules = [];
    protected FilterMatcherSettings? _settings;

    public override void Activate()
    {
        _settings = (this as IModule).LoadSettings<FilterMatcherSettings>();
        WriteLog.Cutely("Filters loaded", [
            ("Filter Count", _settings.RuleList.Count.ToString())
        ]);
    }

    public override void RegisterHooks()
    {
        if (Owner is GuildModule guild)
        {
            guild.OnGuildMessage += MessageCallBack;
            guild.OnGuildMessageEdit += MessageCallBack;
        }
    }

    private async Task MessageCallBack(SocketUserMessage message, SocketGuildChannel channel)
    {

        if (_settings is null)
            return;

        List<Task> tasks = [];

        foreach (var item in _settings.RuleList.Values)
        {
            if (item.Match(message.CleanContent, message.Channel.Id))
            {
                if (item.WarnStaff)
                {
                    if (channel.Guild.GetChannel(_settings.LoggingChannelID) is SocketTextChannel logChannel)
                    {
                        tasks.Add(SendMessageReport(logChannel, message, item));
                    }

                }

                //await message.Channel.SendMessageAsync(text: "Oopsie daisy! (✿◠‿◠) Your message got deleted for using naughty words. Pwease keep it pawsitive and kind! Let's keep our chat fun and fwiendly~ ≧◡≦");
                if (item.SendResponse)
                    tasks.Add(message.Channel.SendMessageAsync(text: item.ResponseMSG));

                if (item.DeleteMessage)
                    tasks.Add(message.DeleteAsync());

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
