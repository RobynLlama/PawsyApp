using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PawsyApp.PawsyCore.Modules;

namespace MeowBoard.Settings;

public class MeowBoardSettings() : ISettings
{
  [JsonInclude]
  internal ConcurrentDictionary<ulong, MeowBank> Records { get; set; } = [];

  [JsonInclude]
  public List<TreasurePoolItem> TreasurePool { get; set; } = [
    new() { BoxName = "🪙 Some Loose Change 🪙", Payout = 50 },
    new() { BoxName = "💷 Stack of Meow Bills 💷", Payout = 100 },
    new() { BoxName = "👛 Purse Full of Meows 👛", Payout = 250 },
    new() { BoxName = "💳 Robyn's Bank Card 💳", Payout = 600 },
    new() { BoxName = "💰 Pile of Meow Money 💰", Payout = 1000 },
    new() { BoxName = "🎖️🏦 Meow Treasure Horde 🏦🎖️", Payout = 2500 }
  ];

  [JsonInclude]
  public List<string> FlavorMessages { get; set; } = [
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

  [JsonInclude]
  internal int MeowBoardDisplayLimit = 5;

  [JsonInclude]
  internal ulong GameChannelID = 0;
}
