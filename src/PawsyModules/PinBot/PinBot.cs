using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using PawsyApp.PawsyCore;
using PawsyApp.PawsyCore.Modules;

using PinBot.Settings;

namespace PinBot;

[PawsyModule(ModuleName)]
public class PinBot : GuildModule
{
    public const string ModuleName = "PinBot";
    protected PinBotSettings Settings;

    public PinBot(Guild Owner) : base(Owner, ModuleName, true, true)
    {
        Settings = (this as ISettingsOwner).LoadSettings<PinBotSettings>();
    }

    public override void OnActivate()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            //Add owner.Event callbacks here
        }
    }

    public override void OnDeactivate()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            //Remove owner.Event callbacks here
        }
    }

    public override SlashCommandBundle OnCommandsDeclared(SlashCommandBuilder builder)
    {

        //Add slash commands here

        builder
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("pin")
            .WithDescription("pin a message (by message link)")
            .WithType(ApplicationCommandOptionType.String)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("unpin")
            .WithDescription("unpin a message (by message link)")
            .WithType(ApplicationCommandOptionType.String)
        );

        return new SlashCommandBundle(ModuleCommandHandler, builder.Build(), Name);
    }

    public override void OnConfigDeclared(SlashCommandOptionBuilder rootConfig)
    {

        //add config options to module-manage here

        rootConfig
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Integer)
            .WithName("meow-limit")
            .WithDescription("Pawsy will meow this many times")
            .WithMaxValue(20)
            .WithMinValue(1)
        );
    }

    public override Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options)
    {
        var option = options.Options.First();
        var optionName = option.Name;
        var optionValue = option.Value;

        switch (optionName)
        {
            case "meow-limit":
                if (optionValue is not int optionMax)
                {
                    return command.RespondAsync("I don't think that's a number, meow!", ephemeral: true);
                }

                Settings.MeowLimit = (int)optionMax;
                (Settings as ISettings).Save<PinBotSettings>(this);

                return command.RespondAsync($"Set meow limit to {optionMax}");
            default:
                return command.RespondAsync("Something went wrong in HandleConfig", ephemeral: true); ;
        }
    }

    private Task ModuleCommandHandler(SocketSlashCommand command)
    {

        if (Settings is null)
            return command.RespondAsync("Settings are null in command handler", ephemeral: true);

        var options = command.Data.Options.First();
        var commandName = options.Name;

        switch (commandName)
        {
            case "meow":
                return command.RespondAsync($"Meow!");
            default:
                return command.RespondAsync("Something went wrong in command handler", ephemeral: true); ;
        }
    }

    // Un-comment this if you need to cleanup before the module is destroyed
    // public override void Destroy() { }
}
