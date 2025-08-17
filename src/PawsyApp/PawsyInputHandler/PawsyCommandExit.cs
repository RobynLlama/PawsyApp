using System;
using SimpleCommandLib;

namespace PawsyApp;

public class PawsyCommandExit : ICommandRunner
{
  public string CommandName => "Shutdown";

  public string CommandUsage => "Shutdown, goodbye!";

  public bool Execute(string[] args)
  {
    Console.WriteLine("Shutting down now");
    PawsyProgram.pawsy.Destroy();
    PawsyProgram.running = false;
    return true;
  }
}
