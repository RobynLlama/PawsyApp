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
    public override string Name => "meow-board";
    public override IModuleSettings? Settings => _settings;

    protected MeowBoardSettings? _settings;
    internal static Emote PawsySmall = new(1277935719805096066, "pawsysmall");

    public override void Alive()
    {
        _settings = (this as IModule).LoadSettings<MeowBoardSettings>();

        if (Owner is GuildModule guild)
        {
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

    public override void OnModuleDeclareConfig(SlashCommandOptionBuilder rootConfig)
    {
        rootConfig
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Integer)
            .WithName("max-display")
            .WithDescription("The maximum number of users Pawsy will show in the /meowboard embed")
            .WithMaxValue(20)
            .WithMinValue(1)
        );
    }

    public override Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options)
    {
        if (_settings is null)
        {
            return command.RespondAsync("Config is unavailable in HandleConfig", ephemeral: true);
        }

        var option = options.Options.First();
        var optionName = option.Name;
        var optionValue = option.Value;

        switch (optionName)
        {
            case "max-display":
                if (optionValue is not long optionMax)
                {
                    return command.RespondAsync("I don't think that's a number, meow!", ephemeral: true);
                }

                _settings.MeowBoardDisplayLimit = (int)optionMax;
                (_settings as IModuleSettings).Save<MeowBoardSettings>();
                return command.RespondAsync($"Set max user display for MeowBoard to {optionMax}");
            default:
                return command.RespondAsync("Something went wrong in HandleConfig", ephemeral: true); ;
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
