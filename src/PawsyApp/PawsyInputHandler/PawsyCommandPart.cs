using System;
using SimpleCommandLib;

namespace PawsyApp;

public class PawsyCommandPart : ICommandRunner
{
  public string CommandName => "Part";

  public string CommandUsage => "Part, permanently leave a guild\n Usage: part uint: GuildID";

  public bool Execute(string[] args)
  {
    if (args.Length < 1)
    {
      Console.WriteLine(CommandUsage);
      return false;
    }

    if (!ulong.TryParse(args[0], out var guildID))
    {
      Console.WriteLine($"Unable to parse GuildID as a number {args[0]}");
      return false;
    }

    PawsyProgram.pawsy.PartFromGuild(guildID);
    return true;
  }
}
