using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Settings;
using System.Collections.Generic;

namespace PawsyApp.PawsyCore.Modules.GuildModules;

internal class MeowBoardModule : GuildModule
{
    protected MeowBoardSettings Settings;
    internal static Emote PawsySmall = new(1277935719805096066, "pawsysmall");

    public MeowBoardModule(Guild Owner) : base(Owner, "meow-board", true, true)
    {
        Settings = (this as ISettingsOwner).LoadSettings<MeowBoardSettings>();
    }

    public override void OnModuleActivation()
    {
        Owner.OnGuildMessage += MessageCallback;
    }

    public override void OnModuleDeactivation()
    {
        Owner.OnGuildMessage -= MessageCallback;
    }

    public override SlashCommandBundle OnModuleDeclareCommands(SlashCommandBuilder builder)
    {
        builder
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("meow")
            .WithDescription("Pawsy will meow for you")
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("display")
            .WithDescription("Pawsy will show you the MeowBoard rankings")
        );

        return new SlashCommandBundle(MeowBoardHandler, builder.Build(), Name);
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

                Settings.MeowBoardDisplayLimit = (int)optionMax;
                (Settings as ISettings).Save<MeowBoardSettings>(this);
                return command.RespondAsync($"Set max user display for MeowBoard to {optionMax}");
            default:
                return command.RespondAsync("Something went wrong in HandleConfig", ephemeral: true); ;
        }
    }

    private Task MeowBoardHandler(SocketSlashCommand command)
    {

        if (Settings is null)
            return command.RespondAsync("Settings are null in MeowBoardHandler", ephemeral: true);

        var options = command.Data.Options.First();
        var commandName = options.Name;

        switch (commandName)
        {
            case "meow":
                return command.RespondAsync($"Meow!"); ;
            case "display":
                return EmbedMeowBoard(command);
            default:
                return command.RespondAsync("Something went wrong in MeowBoardHandler", ephemeral: true); ;
        }
    }

    private Task EmbedMeowBoard(SocketSlashCommand command)
    {
        EmbedBuilder builder = new();
        //WriteLog.Normally("MeowBoard being built");

        var top5 = Settings.Records.OrderByDescending(kvp => kvp.Value)
                    .Take(Settings.MeowBoardDisplayLimit)
                    .ToList();

        static IEnumerable<EmbedFieldBuilder> fields(List<KeyValuePair<ulong, int>> items, SocketGuild guild)
        {
            foreach (var item in items)
            {

                string username;

                if (guild.GetUser(item.Key) is not SocketGuildUser user)
                    continue;
                else
                    username = user.Nickname ?? user.GlobalName ?? user.Username;


                //WriteLog.Normally("Adding a field to the MeowBoard");
                yield return new EmbedFieldBuilder().WithName(username).WithValue(item.Value.ToString());
            }
        }

        builder
            //.WithAuthor("Pawsy!")
            .WithColor(0, 128, 196)
            .WithDescription($"Meow Board top {Settings.MeowBoardDisplayLimit}")
            .WithTitle("Meow Board")
            .WithThumbnailUrl("https://raw.githubusercontent.com/RobynLlama/PawsyApp/main/Assets/img/Pawsy-small.png")
            .WithFields(fields(top5, Owner.DiscordGuild))
            .WithUrl("https://github.com/RobynLlama/PawsyApp")
            .WithCurrentTimestamp();

        //WriteLog.Normally("Responding");

        return command.RespondAsync(embed: builder.Build());
    }

    private void AddUserMeow(ulong userID)
    {
        if (Settings.Records.TryGetValue(userID, out int amount))
            Settings.Records[userID] = amount + 1;
        else
            Settings.Records.TryAdd(userID, 1);

        (Settings as ISettings).Save<MeowBoardSettings>(this);
    }

    private Task MessageCallback(SocketUserMessage message, SocketGuildChannel channel)
    {
        if (message.CleanContent.Contains("meow", System.StringComparison.InvariantCultureIgnoreCase))
        {
            AddUserMeow(message.Author.Id);
            message.AddReactionAsync(PawsySmall);
        }

        return Task.CompletedTask;
    }
}
