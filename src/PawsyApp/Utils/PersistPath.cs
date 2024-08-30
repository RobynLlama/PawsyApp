using System.Reflection;
using System.IO;

namespace PawsyApp.Utils;

internal partial class Helpers
{
    internal static string GetPersistPath()
    {
        return Path.Combine(System.AppContext.BaseDirectory, "PawsyPersist");
    }

    internal static string GetPersistPath(ulong guild)
    {
        return Path.Combine(GetPersistPath(), guild.ToString());
    }
}
