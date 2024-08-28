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

        return Task.CompletedTask;
    }
}
