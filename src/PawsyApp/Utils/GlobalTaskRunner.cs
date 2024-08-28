using System;
using System.Threading.Tasks;

namespace PawsyApp.Utils;

internal static class GlobalTaskRunner
{
    private static async Task HandleTask(Task task)
    {
        try
        {
            await task;
        }
        catch (Exception)
        {
            await WriteLog.Normally("HandleTask encountered an error, discarding");
        }
    }

    internal static void FireAndForget(Task task)
    {
        _ = HandleTask(task);
    }
}
