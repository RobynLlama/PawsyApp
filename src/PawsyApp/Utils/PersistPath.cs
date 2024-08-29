using System.Reflection;
using System.IO;

namespace PawsyApp.Utils;

internal partial class Helpers
{
    internal static string GetPersistPath()
    {
        return Path.Combine(Assembly.GetExecutingAssembly().Location.Replace("PawsyApp.dll", ""), "PawsyPersist");
    }

    internal static string GetPersistPath(ulong guild)
    {
        return Path.Combine(GetPersistPath(), guild.ToString());
    }
}
