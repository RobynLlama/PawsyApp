using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Core;
using PawsyApp.PawsyCore.Modules.Settings;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

internal class MeowBoardModule : GuildSubmodule
{
    public override IModule? Owner { get => _owner; set => _owner = value; }
    public override string Name => "meow-board";
    public override ConcurrentBag<IModule> Modules => _modules;
    public override IModuleSettings? Settings => _settings;

    protected IModule? _owner;
    protected readonly ConcurrentBag<IModule> _modules = [];
    protected MeowBoardSettings? _settings;
    internal static Emote PawsySmall = new(1277935719805096066, "pawsysmall");

    public override void Alive()
    {
        _settings = (this as IModule).LoadSettings<MeowBoardSettings>();

        var ConfigCommand = new SlashCommandBuilder()
        .WithName("meowboard-config")
        .WithDescription("Configure the MeowBoard module")
        .WithDefaultMemberPermissions(GuildPermission.ManageGuild)
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Integer)
            .WithName("max-display")
            .WithDescription("The maximum number of users Pawsy will show in the /meowboard embed")
            .WithMaxValue(20)
            .WithMinValue(1)
        );

        if (Owner is GuildModule guild)
        {
            guild.RegisterSlashCommand(
                new(HandleConfig, ConfigCommand.Build(), Name)
            );

            guild.RegisterSlashCommand(
                new(CommandMeowBoard,
                new SlashCommandBuilder().
                WithName("meowboard")
                .WithDescription("View the MeowBoard")
                .Build(),
                Name)
            );

            guild.RegisterSlashCommand(
                new(CommandMeow,
                new SlashCommandBuilder()
                .WithName("meow")
                .WithDescription("Pawsy will meow for you")
                .Build(),
                Name)
            );

        }
    }

    public override void OnModuleActivation()
    {
        if (_owner is GuildModule guild)
        {
            guild.OnGuildMessage += MessageCallback;
        }
    }

    public override void OnModuleDeactivation()
    {
        if (_owner is GuildModule guild)
        {
            guild.OnGuildMessage -= MessageCallback;
        }
    }

    private async Task HandleConfig(SocketSlashCommand command)
    {

        if (_settings is null)
        {
            await command.RespondAsync("Config is unavailable in HandleConfig", ephemeral: true);
            return;
        }

        var option = command.Data.Options.First();
        var optionName = option.Name;
        var optionValue = option.Value;

        switch (optionName)
        {
            case "max-display":
                if (optionValue is not long optionMax)
                {
                    await command.RespondAsync("I don't think that's a number, meow!", ephemeral: true);
                    return;
                }

                _settings.MeowBoardDisplayLimit = (int)optionMax;
                (_settings as IModuleSettings).Save<MeowBoardSettings>();
                await command.RespondAsync($"Set max user display for MeowBoard to {optionMax}");
                return;
            default:
                await command.RespondAsync("Something went wrong in HandleConfig", ephemeral: true);
                return;
        }
    }

    private Task CommandMeowBoard(SocketSlashCommand command)
    {
        if (_settings is not null)
            return _settings.EmbedMeowBoard(command);

        return command.RespondAsync("Something went wrong, meow!", ephemeral: true);
    }

    private Task CommandMeow(SocketSlashCommand command)
    {
        return command.RespondAsync($"Meow!");
    }

    private Task MessageCallback(SocketUserMessage message, SocketGuildChannel channel)
    {
        if (message.CleanContent.Contains("meow"))
        {
            _settings?.AddUserMeow(message.Author.Id);
            message.AddReactionAsync(PawsySmall);
        }

        return Task.CompletedTask;
    }
}
