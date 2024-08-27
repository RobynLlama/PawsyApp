using System.Reflection;
using System.IO;

namespace PawsyApp.Utils;

internal class GuildFile
{
    internal static string Get(ulong GuildID)
    {
        return Path.Combine(Assembly.GetExecutingAssembly().Location.Replace("PawsyApp.dll", ""), "PawsyPersist", $"{GuildID}.json");
    }
}
