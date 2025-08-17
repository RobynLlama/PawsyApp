using System;
using System.Threading.Tasks;
using PawsyApp.PawsyCore;

namespace PawsyApp;

public partial class PawsyProgram
{

  private readonly static PawsyInput handler = new();

  public static async Task HandleInputAsync()
  {
    var input = Console.In;
    await Task.Delay(1);

    while (true)
    {
      var line = await input.ReadLineAsync();

      if (line is null)
        continue;

      handler.ParseAndRunCommand(line);
    }
  }
}
