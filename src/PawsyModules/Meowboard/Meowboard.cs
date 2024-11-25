using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using PawsyApp.PawsyCore;
using PawsyApp.PawsyCore.Modules;

using MeowBoard.Settings;

namespace MeowBoard;

[PawsyModule]
public class MeowBoardModule : GuildModule
{
    protected MeowBoardSettings Settings;
    protected TreasureHunter TreasureGame;
    private readonly Task UpdateLoop;
    internal static Emote PawsySmall = new(1277935719805096066, "pawsysmall");
    protected bool Enabled = false;

    public MeowBoardModule(Guild Owner) : base(Owner, "meow-board", true, true)
    {
        Settings = (this as ISettingsOwner).LoadSettings<MeowBoardSettings>();
        TreasureGame = new(this);
        UpdateLoop = UpdateGameState();
    }

    private async Task UpdateGameState()
    {
        while (true)
        {
            await Task.Delay(2500);

            if (Enabled)
                TreasureGame.UpdateGamePhase();
        };
    }

    public override void OnActivate()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            owner.OnGuildMessage += MessageCallback;
            owner.OnGuildButtonClicked += ButtonCallback;
            Enabled = true;
        }
    }

    public override void OnDeactivate()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            owner.OnGuildMessage -= MessageCallback;
            owner.OnGuildButtonClicked -= ButtonCallback;
            Enabled = false;
        }
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
                return command.RespondAsync($"Your balance is {acc.MeowMoney} Meows", ephemeral: true);
            default:
                return command.RespondAsync("Something went wrong in MeowBoardHandler", ephemeral: true); ;
        }
    }

    private Task EmbedMeowBoard(SocketSlashCommand command)
    {

        if (!Owner.TryGetTarget(out var owner))
            throw new Exception("Owner is null in EmbedMeowBoard");

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
            .WithFields(fields(top5, owner.DiscordGuild))
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
        protected WeakReference<MeowBoardModule> Owner = new(Owner);
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
            if (!Owner.TryGetTarget(out var meowOwner))
                throw new("MeowBoard doesn't real in UpdateGamePhase");

            if (!meowOwner.Owner.TryGetTarget(out var owner))
                throw new("Owner doesn't real in UpdateGamePhase");

            try
            {
                if (owner.DiscordGuild.GetChannel(meowOwner.Settings.GameChannelID) is not SocketTextChannel gameChannel)
                    return;

                if (GameActive)
                {
                    if (DateTime.Now > GameEndsAt)
                    {

                        if (TreasureHunters.IsEmpty)
                        {
                            return;
                        }

                        //Reset
                        NextGameAt = DateTime.Now.AddSeconds(150f + (new Random().NextSingle() * 30));
                        GameActive = false;

                        string Claimers = "Claimed by:";
                        var (Box, TreasureValue) = GetTreasureType();
                        MeowBank account;

                        foreach (var item in TreasureHunters)
                        {
                            Claimers += $" <@{item}>";
                            account = meowOwner.GetUserAccount(item);
                            account.MeowMoney += TreasureValue;

                            if (FirstResponder == item)
                            {
                                account.MeowMoney += 100;
                            }
                        }

                        (meowOwner.Settings as ISettings).Save<MeowBoardSettings>(meowOwner);

                        //Modify
                        if (gameMessage is not null)
                            await gameMessage.ModifyAsync(msg => { msg.Content = $"{Box}\nWorth {TreasureValue} Meows\nFirst Clicker Bonus <@{FirstResponder}> (+100)\n{Claimers}"; msg.Components = null; });

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
                        GameEndsAt = DateTime.Now.AddSeconds(45f);
                    }
                }
            }
            catch (Exception)
            {
                await meowOwner.LogAppendLine("Unable to update game phase");
            }
        }
        public async void AddClicker(SocketMessageComponent component)
        {
            if (component.HasResponded)
                return;

            var ID = component.User.Id;
            try
            {

                if (component.Message.Id != gameMessage?.Id)
                {
                    await component.RespondAsync("You somehow found an old treasure I should have deleted. Thanks!", ephemeral: true);
                    await component.Message.DeleteAsync();
                    return;
                }

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
                    await component.RespondAsync("It may take up to a minute to open this treasure box, please wait.", ephemeral: true);
                }

                UpdateGamePhase();
            }
            catch (Exception)
            {
                //Discard
            }
        }

        protected static (string BoxType, ulong TreasureAmount) GetTreasureType()
        {
            var number = new Random().NextSingle();
            var Box = "🪙 Some Loose Change 🪙";
            ulong TreasureAmount = 50;

            if (number > 0.985f)
            {
                Box = "🎖️🏦 Meow Treasure Horde 🏦🎖️";
                TreasureAmount = 2500;
            }
            else if (number > 0.90f)
            {
                Box = "💰 Pile of Meow Money 💰";
                TreasureAmount = 1000;
            }
            else if (number > 0.75f)
            {
                Box = "💳 Robyn's Bank Card 💳";
                TreasureAmount = 600;
            }
            else if (number > 0.55f)
            {
                Box = "👛 Purse Full of Meows 👛";
                TreasureAmount = 250;
            }
            else if (number > 0.25f)
            {
                Box = "💷 Stack of Meow Bills 💷";
                TreasureAmount = 100;
            }

            return (Box, TreasureAmount);
        }
    }

    private async Task ButtonCallback(SocketMessageComponent component)
    {

        switch (component.Data.CustomId)
        {
            case "meow-board-claim-button":
                {
                    try
                    {
                        TreasureGame.AddClicker(component);
                    }
                    catch (Exception)
                    {
                        await LogAppendLine("Interaction failed");
                    }

                    return;
                }

        }

        return;
    }

    private Task MessageCallback(SocketUserMessage message, SocketGuildChannel channel)
    {
        return Task.CompletedTask;
    }
}
