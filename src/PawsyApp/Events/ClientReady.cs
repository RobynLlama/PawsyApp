using PawsyApp.Events.SlashCommands;
using PawsyApp.Utils;
using System.Threading.Tasks;

namespace PawsyApp.Events;

internal class ClientReady
{
    internal static Task Respond()
    {
        WriteLog.Normally("Building global commands");

        //Kills all our global commands cuz I added a bunch on accident
        PawsyProgram._client?.BulkOverwriteGlobalApplicationCommandsAsync([]);

        //Add a meow command
        var meow = new SlashMeow();
        meow.RegisterSlashCommand();

        PawsyProgram._client?.CreateGlobalApplicationCommandAsync(meow.BuiltCommand);

        return Task.CompletedTask;
    }
}
