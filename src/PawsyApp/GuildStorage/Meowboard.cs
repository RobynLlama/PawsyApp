using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PawsyApp.GuildStorage;

internal class MeowBoard
{
    [JsonInclude]
    internal ConcurrentDictionary<ulong, int> Records { get; set; } = [];
    public MeowBoard() { }

    public void AddUserMeow(ulong userID)
    {
        if (Records.TryGetValue(userID, out int amount))
            Records[userID] = amount + 1;
        else
            Records.TryAdd(userID, 1);
    }

    public Task EmbedMeowBoard(SocketSlashCommand command)
    {
        return Task.CompletedTask;
    }
}
