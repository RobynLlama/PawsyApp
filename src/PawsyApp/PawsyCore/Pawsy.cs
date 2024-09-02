using System.Collections.Concurrent;
using System;
using System.IO;
using System.Collections.Generic;
using PawsyApp.Utils;
using Discord.WebSocket;

namespace PawsyApp.PawsyCore;

internal class Pawsy : IUniqueCollection<ulong>
{
    //Eventually migrate socket connection here
    public static string BaseConfigDir { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pawsy");
    protected ConcurrentBag<Guild> Guilds = [];
    public IEnumerable<IUnique<ulong>> UniqueCollection => Guilds;

    public Pawsy()
    {
        DirectoryInfo config = new(BaseConfigDir);
        if (!config.Exists)
            config.Create();
    }

    public Guild AddOrGetGuild(SocketGuild guild)
    {
        var ID = guild.Id;
        var item = (this as IUniqueCollection<ulong>).GetUniqueItem(ID);

        if (item is null)
        {
            WriteLog.LineNormal($"Creating Guild Instance for {ID}");
            Guild newItem = new(guild);
            Guilds.Add(newItem);
            return newItem;
        }

        if (item is Guild gItem)
        {
            WriteLog.LineNormal($"Returning Guild Instance for {ID}");
            return gItem;
        }

        throw new Exception($"Unreachable code in AddOrGetGuild. ID: {ID}");
    }
}
