using System.IO;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;
using PawsyApp.PawsyCore.Modules.GuildSubmodules;
using System.Collections.Concurrent;
using Discord.WebSocket;
using System.Threading.Tasks;
using System;
using Discord;

namespace PawsyApp.PawsyCore.Modules.Core;

internal class GuildModule() : IModuleIdent
{
    public IModule? Owner { get => _owner; set => _owner = value; }
    public ConcurrentBag<IModule> Modules => _modules;
    public string Name { get => "GuildGlobal"; }
    public ulong ID { get => _id; set => _id = value; }
    IModuleSettings? IModule.Settings => Settings;
    public string GetSettingsLocation() =>
    Path.Combine(Helpers.GetPersistPath(ID), $"{Name}.json");

    protected ulong _id;
    protected IModule? _owner;
    protected readonly ConcurrentBag<IModule> _modules = [];
    protected GuildSettings? Settings;

    public delegate Task GuildMessageHandler(SocketUserMessage message, SocketGuildChannel channel);
    public event GuildMessageHandler? OnGuildMessage;
    public event GuildMessageHandler? OnGuildMessageEdit;

    public void Activate()
    {
        Settings = (this as IModule).LoadSettings<GuildSettings>();

        //Decide if we should activate modules here
        (this as IModule).AddModule<MeowBoardModule>();
        (this as IModule).AddModule<FilterMatcherModule>();

        //Subscribe to events
        PawsyProgram.SocketClient.MessageReceived += OnMessage;
        PawsyProgram.SocketClient.MessageUpdated += OnMessageEdit;
    }

    private async Task OnMessage(SocketMessage message)
    {
        //Filter out bots, system and webhook message
        if (message.Author.IsBot || message.Author.IsWebhook || message.Source == MessageSource.System)
            return;

        if (message is SocketUserMessage uMessage)
        {
            //Guild messages
            if (OnGuildMessage is not null && message.Channel is SocketGuildChannel gChannel && gChannel.Guild.Id == ID)
                await OnGuildMessage(uMessage, gChannel);
        }
    }

    private async Task OnMessageEdit(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel channel)
    {
        //Filter out bots and system messages
        if (message is SocketUserMessage uMessage)
        {
            //Guild messages
            if (OnGuildMessageEdit is not null && message.Channel is SocketGuildChannel gChannel && gChannel.Guild.Id == ID)
                await OnGuildMessageEdit(uMessage, gChannel);
        }
    }
}
