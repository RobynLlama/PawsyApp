using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MeowBoard.Settings;
using PawsyApp.PawsyCore;
using PawsyApp.PawsyCore.Modules;

namespace MeowBoard;

[PawsyModule(ModuleName)]
public class MeowBoardModule : GuildModule
{
  public const string ModuleName = "meow-board";
  protected MeowBoardSettings Settings;
  protected TreasureHunter TreasureGame;
  private readonly Task UpdateLoop;
  internal static Emote PawsySmall = new(1277935719805096066, "pawsysmall");
  protected bool Enabled = false;

  public MeowBoardModule(Guild Owner) : base(Owner, ModuleName, true, true)
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
    }
    ;
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

    TreasureGame.DeleteCurrentTreasureMessage();
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
            .WithName("restart-game")
            .WithDescription("If Pawsy has lost track of the treasure game this will send a new message")
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
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("edit-treasure")
            .WithDescription("Add/remove a treasure to the list.")
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithName("action")
                .WithDescription("Add or Remove")
                .AddChoice("Add", "add")
                .AddChoice("Remove", "remove")
                .WithRequired(true)
            )
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithName("name")
                .WithDescription("The name of the treasure box")
                .WithRequired(true)
            )
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.Integer)
                .WithName("payout")
                .WithDescription("The Meow reward for this box (required for Add)")
                .WithRequired(false)
            )
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("edit-message")
            .WithDescription("Add/remove a treasure message.")
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithName("action")
                .WithDescription("Add or Remove")
                .AddChoice("Add", "add")
                .AddChoice("Remove", "remove")
                .WithRequired(true)
            )
            .AddOption(
                new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithName("text")
                .WithDescription("The message text")
                .WithRequired(true)
            )
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName("list-all")
            .WithDescription("List all treasure messages and treasures")
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
    var subCommand = options.Options.First();
    var optionName = subCommand.Name;

    switch (optionName)
    {
      case "max-display":
      if (subCommand.Value is not long optionMax)
        return command.RespondAsync("I don't think that's a number, meow!", ephemeral: true);

        Settings.MeowBoardDisplayLimit = (int)optionMax;
        (Settings as ISettings).Save<MeowBoardSettings>(this);
        return command.RespondAsync($"Set max user display for MeowBoard to {optionMax}");

      case "game-channel":
        if (subCommand.Value is not SocketTextChannel optionChannel)
        return command.RespondAsync("Text channels only, please and thank mew", ephemeral: true);

        Settings.GameChannelID = optionChannel.Id;
        (Settings as ISettings).Save<MeowBoardSettings>(this);
        return command.RespondAsync($"Set game channel to <#{optionChannel.Id}>");
      default:
        return command.RespondAsync("Something went wrong in HandleConfig", ephemeral: true);
    }
  }

  private async Task MeowBoardHandler(SocketSlashCommand command)
  {
    if (Settings is null)
    {
      await command.RespondAsync("Settings are null in MeowBoardHandler", ephemeral: true);
      return;
    }

    await command.DeferAsync(ephemeral: true);

    var subCommand = command.Data.Options.First();
    var commandName = subCommand.Name;

    switch (commandName)
    {
      case "meow":
        await command.FollowupAsync("Meow!");
        break;
      case "display":
        await EmbedMeowBoard(command);
        break;
      case "restart-game":
        if (command.User is SocketGuildUser user && command.Channel is SocketGuildChannel channel)
        {
          if (user.GetPermissions(channel).ManageMessages)
          {
            TreasureGame.ResetTreasureGame();
            await command.FollowupAsync("Resetting treasure game");
          }
          else
          {
            await command.FollowupAsync("You must at least be a moderator to run this command, meow");
          }
        }
        break;
      case "my-bank":
        var acc = GetUserAccount(command.User.Id);
        await command.FollowupAsync($"Your balance is {acc.MeowMoney} Meows");
        break;
      case "edit-treasure":
      {
        if (command.User is SocketGuildUser CmdUser && command.Channel is SocketGuildChannel Cmdchannel)
        {
          if (!CmdUser.GuildPermissions.ManageMessages)
          {
            await command.FollowupAsync("You must at least be a moderator to run this command, meow");
            return;
          }
        }
        var action = subCommand.Options.First(x => x.Name == "action").Value.ToString();
        var name = subCommand.Options.First(x => x.Name == "name").Value.ToString();

        if (action == "add")
        {
          var payoutOption = subCommand.Options.FirstOrDefault(x => x.Name == "payout");
          if (payoutOption == null)
          {
            await command.FollowupAsync("Payout is required to add!");
            return;
          }

          if (Settings.TreasurePool.Any(x => x.BoxName.Equals(name, StringComparison.OrdinalIgnoreCase)))
          {
            await command.FollowupAsync("That treasure already exists in the pool.");
            return;
          }

          var payout = (long)payoutOption.Value;
          Settings.TreasurePool.Add(new TreasurePoolItem { BoxName = name, Payout = (ulong)payout });
          (Settings as ISettings).Save<MeowBoardSettings>(this);
          await command.FollowupAsync($"Added **{name}** ({payout} Meows) to the loot pool!");
        }
        else
        {
          var item = Settings.TreasurePool.FirstOrDefault(x => x.BoxName.Equals(name, StringComparison.OrdinalIgnoreCase));
          if (item == null)
          {
            await command.FollowupAsync("Couldn't find a treasure with that name.");
            return;
          }

          Settings.TreasurePool.Remove(item);
          (Settings as ISettings).Save<MeowBoardSettings>(this);
          await command.FollowupAsync($"Removed **{name}** from the pool.");
        }
        break;
      }
      case "edit-message":
      {
        if (command.User is SocketGuildUser CmdUser && command.Channel is SocketGuildChannel Cmdchannel)
        {
          if (!CmdUser.GuildPermissions.ManageMessages)
          {
            await command.FollowupAsync("You must at least be a moderator to run this command, meow");
            return;
          }
        }
          
        var action = subCommand.Options.First(x => x.Name == "action").Value.ToString();
        var text = subCommand.Options.First(x => x.Name == "text").Value.ToString();

        if (action == "add")
        {
          if (Settings.FlavorMessages.Contains(text))
          {
            await command.FollowupAsync("That message already exists in the list.");
            return;
          }

          Settings.FlavorMessages.Add(text);
          (Settings as ISettings).Save<MeowBoardSettings>(this);
          await command.FollowupAsync($"Added message: \"{text}\"");
        }
        else
        {
          if (Settings.FlavorMessages.Contains(text))
          {
            Settings.FlavorMessages.Remove(text);
            (Settings as ISettings).Save<MeowBoardSettings>(this);
            await command.FollowupAsync("Message removed.");
          }
          else
          {
            await command.FollowupAsync("Message not found in list.");
          }
        }
        break;
      }
      case "list-all":
        await ListAllConfig(command);
        break;
      default:
        await command.FollowupAsync("Something went wrong in MeowBoardHandler");
        break;
      }
  }

  private async Task ListAllConfig(SocketSlashCommand command)
  {
    var embed = new EmbedBuilder()
      .WithTitle("MeowBoard Configuration")
      .WithColor(Color.Blue)
      .WithThumbnailUrl("https://raw.githubusercontent.com/RobynLlama/PawsyApp/main/Assets/img/Pawsy-small.png?version-2");

    var treasures = string.Join("\n", Settings.TreasurePool.Select(t => $"- **{t.BoxName}**: {t.Payout} Meows"));
    embed.AddField("Treasures in Pool", string.IsNullOrWhiteSpace(treasures) ? "None" : treasures);

    var messages = string.Join("\n", Settings.FlavorMessages.Select(m => $"- {m}"));
    if (messages.Length > 1024) messages = messages.Substring(0, 1020) + "...";

    embed.AddField("Treasure Spawn Messages", string.IsNullOrWhiteSpace(messages) ? "None" : messages);
    await command.FollowupAsync(embed: embed.Build());
  }



  public override void Destroy()
  {
    TreasureGame.DeleteCurrentTreasureMessage();
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
    protected ulong FirstResponder = 0u;
    protected bool GameActive = false;
    public object LockRoot = new();
    internal DateTime NextGameAt = DateTime.Now.AddSeconds(10f);
    protected DateTime GameEndsAt = DateTime.Now.AddSeconds(10f);
    protected RestUserMessage? gameMessage;
    protected int currentLine = 0;
    protected string currentBoxName = "";
    protected ulong currentPayout = 0;

    internal void DeleteCurrentTreasureMessage()
    {
      if (gameMessage is null)
        return;

      if (!Owner.TryGetTarget(out var owner))
        return;

      owner.LogAppendLine("Deleting a treasure game message");
      gameMessage.DeleteAsync();
      gameMessage = null;
    }

    internal void ResetTreasureGame()
    {
      TreasureHunters = [];
      DeleteCurrentTreasureMessage();
      GameActive = false;
      NextGameAt = DateTime.Now.AddSeconds(10f);
      GameEndsAt = DateTime.Now.AddSeconds(10f);
      FirstResponder = 0u;
    }

    protected (string BoxName, ulong Payout) RollForTreasure(List<TreasurePoolItem> pool)
    {
      // High payouts were very rare in testing, so to make them more common lower the variable below.
      double flatteningFactor = 0.8;

      double totalWeight = pool.Sum(t => 1.0 / Math.Pow((double)t.Payout, flatteningFactor));
      double roll = new Random().NextDouble() * totalWeight;
      double cursor = 0;

      foreach (var item in pool)
      {
        cursor += (1.0 / Math.Pow((double)item.Payout, flatteningFactor));
        if (cursor >= roll) return (item.BoxName, item.Payout);
      }
      return (pool[0].BoxName, pool[0].Payout);
    }

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

          if (TreasureHunters.IsEmpty)
          {
            return;
          }
            if (DateTime.Now > GameEndsAt)
            {
              NextGameAt = DateTime.Now.AddSeconds(150f + (new Random().NextSingle() * 30));
              GameActive = false;

              string Claimers = "Claimed by:";
              foreach (var item in TreasureHunters)
              {
                Claimers += $" <@{item}>";
                var acc = meowOwner.GetUserAccount(item);
                acc.MeowMoney += currentPayout;
                if (FirstResponder == item) acc.MeowMoney += 100;
              }

              (meowOwner.Settings as ISettings).Save<MeowBoardSettings>(meowOwner);

              if (gameMessage is not null)
                await gameMessage.ModifyAsync(msg => {
                  msg.Content = $"{currentBoxName}\nWorth {currentPayout} Meows\nFirst Clicker Bonus <@{FirstResponder}> (+100)\n{Claimers}";
                  msg.Components = null;
                });
              gameMessage = null;
            }
        }
        else
        {
          if (DateTime.Now > NextGameAt)
          {
            var msgList = meowOwner.Settings.FlavorMessages;
            string rollMsg = msgList[new Random().Next(msgList.Count)];

            // Roll for treasure value (Higher Payout = Lower Weight)
            var rolled = RollForTreasure(meowOwner.Settings.TreasurePool);
            currentBoxName = rolled.BoxName;
            currentPayout = rolled.Payout;

            TreasureHunters.Clear();
            FirstResponder = 0;
            gameMessage = await gameChannel.SendMessageAsync(rollMsg, components: claimButton);
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
