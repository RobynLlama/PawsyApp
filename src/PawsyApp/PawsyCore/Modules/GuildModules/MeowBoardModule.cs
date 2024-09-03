using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.PawsyCore.Modules.Settings;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using Discord.Rest;

namespace PawsyApp.PawsyCore.Modules.GuildModules;

internal class MeowBoardModule : GuildModule
{
    protected MeowBoardSettings Settings;
    protected TreasureHunter TreasureGame;
    internal static Emote PawsySmall = new(1277935719805096066, "pawsysmall");

    public MeowBoardModule(Guild Owner) : base(Owner, "meow-board", true, true)
    {
        Settings = (this as ISettingsOwner).LoadSettings<MeowBoardSettings>();
        TreasureGame = new(this);
    }

    public override void OnActivate()
    {
        Owner.OnGuildMessage += MessageCallback;
        Owner.OnGuildButtonClicked += ButtonCallback;
    }

    public override void OnDeactivate()
    {
        Owner.OnGuildMessage -= MessageCallback;
        Owner.OnGuildButtonClicked -= ButtonCallback;
    }

    public override SlashCommandBundle OnCommandsDeclared(SlashCommandBuilder builder)
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
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("my-bank")
            .WithDescription("Pawsy will show you your meow balance")
        );

        return new SlashCommandBundle(MeowBoardHandler, builder.Build(), Name);
    }

    public override void OnConfigDeclared(SlashCommandOptionBuilder rootConfig)
    {
        rootConfig
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Integer)
            .WithName("max-display")
            .WithDescription("The maximum number of users Pawsy will show in the /meowboard embed")
            .WithMaxValue(20)
            .WithMinValue(1)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Channel)
            .WithName("game-channel")
            .WithDescription("The channel Pawsy will offer games in for Meow Money")
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
            case "game-channel":
                if (optionValue is not SocketTextChannel optionChannel)
                {
                    return command.RespondAsync("Text channels only, please and thank mew", ephemeral: true);
                }

                Settings.GameChannelID = optionChannel.Id;
                (Settings as ISettings).Save<MeowBoardSettings>(this);

                return command.RespondAsync($"Set game channel to <#{optionChannel.Id}>");
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
            case "my-bank":
                var acc = GetUserAccount(command.User.Id);
                return command.RespondAsync($"Your balance is {acc.MeowMoney} Meows");
            default:
                return command.RespondAsync("Something went wrong in MeowBoardHandler", ephemeral: true); ;
        }
    }

    private Task EmbedMeowBoard(SocketSlashCommand command)
    {
        EmbedBuilder builder = new();
        //WriteLog.Normally("MeowBoard being built");

        var top5 = Settings.Records.OrderByDescending(kvp => kvp.Value.MeowMoney)
                    .Take(Settings.MeowBoardDisplayLimit)
                    .ToList();

        static IEnumerable<EmbedFieldBuilder> fields(List<KeyValuePair<ulong, MeowBank>> items, SocketGuild guild)
        {
            foreach (var item in items)
            {

                string username;

                if (guild.GetUser(item.Key) is not SocketGuildUser user)
                    continue;
                else
                    username = user.Nickname ?? user.GlobalName ?? user.Username;


                //WriteLog.Normally("Adding a field to the MeowBoard");
                yield return new EmbedFieldBuilder().WithName(username).WithValue(item.Value.MeowMoney.ToString());
            }
        }

        builder
            //.WithAuthor("Pawsy!")
            .WithColor(0, 128, 196)
            .WithDescription($"Meow Board top {Settings.MeowBoardDisplayLimit}")
            .WithTitle("Meow Board")
            .WithThumbnailUrl("https://raw.githubusercontent.com/RobynLlama/PawsyApp/main/Assets/img/Pawsy-small.png?version-2")
            .WithFields(fields(top5, Owner.DiscordGuild))
            .WithUrl("https://github.com/RobynLlama/PawsyApp")
            .WithCurrentTimestamp();

        //WriteLog.Normally("Responding");

        return command.RespondAsync(embed: builder.Build());
    }

    private MeowBank GetUserAccount(ulong userID)
    {
        if (Settings.Records.TryGetValue(userID, out MeowBank? account))
            return account;

        var nAcc = new MeowBank();
        if (Settings.Records.TryAdd(userID, nAcc))
        {
            (Settings as ISettings).Save<MeowBoardSettings>(this);
            return nAcc;
        }

        throw new Exception($"Unable to initialize MeowBank for user {userID}");
    }

    private void AddUserMeows(ulong userID, ulong meows)
    {
        var acc = GetUserAccount(userID);
        acc.MeowMoney += meows;

        (Settings as ISettings).Save<MeowBoardSettings>(this);
    }

    private static readonly MessageComponent claimButton = new ComponentBuilder()
    .WithButton(
        new ButtonBuilder()
        .WithCustomId("meow-board-claim-button")
        .WithLabel("🎁 Claim Reward 🎁")
        .WithStyle(ButtonStyle.Success)
    )
    .Build();

    protected class TreasureHunter(MeowBoardModule Owner)
    {
        protected MeowBoardModule Owner = Owner;
        protected ConcurrentBag<ulong> TreasureHunters = [];
        protected ulong FirstResponder = 0;
        protected bool GameActive = false;
        public object LockRoot = new();
        internal DateTime NextGameAt = DateTime.Now.AddSeconds(10f);
        protected DateTime GameEndsAt = DateTime.Now.AddSeconds(10f);
        protected RestUserMessage? gameMessage;
        protected int currentLine = 0;

        protected string[] TreasureMessages = [
            "A truly meow-tastic treasure has appeared!",
            "I found a treasure for you!",
            "Hurry, open up this treasure, nya~",
            "Meow meow, I found this, open it, okay?",
            "Pawsitively purr-fect treasure awaits you!",
            "Look what I dug up for you, nyan-tastic, right?",
            "A whisker-licking good find just for you!",
            "Meow-gical treasure discovered! Open it now!",
            "This gem is the cat's pajamas, hurry and see!",
            "Purr-haps you'd like to see this shiny surprise?",
            "I sniffed out something special for you, nya~",
            "Unleash the meow-velous magic inside this box!",
            "I've got a _feline_ you'll love this!",
            "A cat-tastic discovery! Open it up and enjoy!"
        ];

        public async void UpdateGamePhase()
        {
            if (Owner.Owner.DiscordGuild.GetChannel(Owner.Settings.GameChannelID) is not SocketTextChannel gameChannel)
                return;

            if (GameActive)
            {
                if (DateTime.Now > GameEndsAt)
                {
                    //Reset
                    NextGameAt = DateTime.Now.AddSeconds(85f);
                    GameActive = false;

                    //delete old message
                    if (gameMessage is not null)
                        await gameMessage.DeleteAsync();

                    if (TreasureHunters.IsEmpty)
                        return;

                    string Claimers = "Claimed by:";
                    var (Box, TreasureValue) = GetTreasureType();
                    MeowBank account;

                    foreach (var item in TreasureHunters)
                    {
                        Claimers += $" <@{item}>";
                        account = Owner.GetUserAccount(item);
                        account.MeowMoney += TreasureValue;

                        if (FirstResponder == item)
                        {
                            account.MeowMoney += 25;
                        }
                    }

                    (Owner.Settings as ISettings).Save<MeowBoardSettings>(Owner);
                    await gameChannel.SendMessageAsync($"{Box}\nWorth {TreasureValue} Meows\nFirst Clicker Bonus <@{FirstResponder}> (+25)\n{Claimers}", allowedMentions: AllowedMentions.None);
                }
            }
            else
            {
                if (DateTime.Now > NextGameAt)
                {

                    currentLine++;
                    currentLine %= TreasureMessages.Length;

                    //Send the message and start the countdown
                    TreasureHunters.Clear();
                    FirstResponder = 0;
                    gameMessage = await gameChannel.SendMessageAsync(TreasureMessages[currentLine], components: claimButton);
                    GameActive = true;
                    GameEndsAt = DateTime.Now.AddSeconds(10f);
                }
            }
        }
        public async void AddClicker(SocketMessageComponent component)
        {
            var ID = component.User.Id;

            if (TreasureHunters.Contains(ID))
            {
                await component.RespondAsync("You're still opening this treasure, be patient meow!", ephemeral: true);
            }
            else
            {
                //First responders
                if (FirstResponder == 0)
                    FirstResponder = ID;

                TreasureHunters.Add(ID);
                await component.RespondAsync("You're opening up this treasure, wait a few seconds to find out what it is!", ephemeral: true);
            }

            UpdateGamePhase();
        }

        protected static (string BoxType, ulong TreasureAmount) GetTreasureType()
        {
            var number = new Random().NextSingle();
            var Box = "🪙 Some Loose Change 🪙";
            ulong TreasureAmount = 5;

            if (number > 0.985f)
            {
                Box = "🎖️🏦 Meow Treasure Horde 🏦🎖️";
                TreasureAmount = 1000;
            }
            else if (number > 0.94f)
            {
                Box = "💰 Pile of Meow Money 💰";
                TreasureAmount = 350;
            }
            else if (number > 0.78f)
            {
                Box = "👛 Purse Full of Meows 👛";
                TreasureAmount = 100;
            }
            else if (number > 0.28f)
            {
                Box = "💷 Stack of Meow Bills 💷";
                TreasureAmount = 25;
            }
            else
            {
                TreasureAmount = 5;
            }

            return (Box, TreasureAmount);
        }
    }

    private Task ButtonCallback(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "meow-board-claim-button":
                TreasureGame.AddClicker(component);
                return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    private Task MessageCallback(SocketUserMessage message, SocketGuildChannel channel)
    {
        lock (TreasureGame.LockRoot)
            if (Settings.GameChannelID != 0)
                TreasureGame.UpdateGamePhase();

        return Task.CompletedTask;
    }
}
