using System.Collections.Concurrent;
using System.Linq;

namespace PawsyApp.PawsyCore;

internal class Pawsy()
{
    //Eventually migrate socket connection here
    protected ConcurrentBag<Guild> Guilds = [];

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
