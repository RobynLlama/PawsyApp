using System.Collections.Concurrent;
using System;
using System.IO;

namespace PawsyApp.PawsyCore;

internal class Pawsy
{
    //Eventually migrate socket connection here
    public static string BaseConfigDir { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pawsy");
    protected ConcurrentBag<Guild> Guilds = [];

    public Pawsy()
    {
        DirectoryInfo config = new(BaseConfigDir);
        if (!config.Exists)
            config.Create();
    }

    public void AddGuild(ulong ID)
    {
        foreach (var item in Guilds)
        {
            if (item.ID == ID)
                return;
        }

        Guilds.Add(new(ID));
    }
}
