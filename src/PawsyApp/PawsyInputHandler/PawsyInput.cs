using System;
using System.Collections.Generic;
using SimpleCommandLib;

namespace PawsyApp;

public class PawsyInput : CommandDispatcher
{
  protected override Dictionary<string, ICommandRunner> CommandsMap { get => _commands; set { } }
  private readonly Dictionary<string, ICommandRunner> _commands = new(StringComparer.OrdinalIgnoreCase);

  public PawsyInput()
  {
    TryAddCommand(new PawsyCommandPart());
  }

  public override void OnCommandNotFound(string commandName)
  {
    Console.WriteLine($"Command not found: {commandName}");
  }
}
