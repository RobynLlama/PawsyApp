using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.Utils;

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

    public Task EmbedMeowBoard(SocketSlashCommand command, GuildSettings settings)
    {

        if (CommonGetters.GetGuild(command.GuildId) is not SocketGuild guild)
        {
            command.RespondAsync("Something went wrong", ephemeral: true);
            return Task.CompletedTask;
        }


        EmbedBuilder builder = new();
        //WriteLog.Normally("MeowBoard being built");

        var top5 = Records.OrderByDescending(kvp => kvp.Value)
                    .Take(5)
                    .ToList();

        static IEnumerable<EmbedFieldBuilder> fields(List<KeyValuePair<ulong, int>> items, SocketGuild guild)
        {
            foreach (var item in items)
            {
                var user = guild.GetUser(item.Key);
                var username = user.Nickname ?? user.GlobalName ?? user.Username;
                //WriteLog.Normally("Adding a field to the MeowBoard");
                yield return new EmbedFieldBuilder().WithName(username).WithValue(item.Value.ToString());
            }
        }

        builder
            .WithAuthor("Pawsy!")
            .WithColor(0, 128, 196)
            .WithDescription("MeowBoard top 5")
            .WithTitle("MeowBoard")
            .WithFields(fields(top5, guild));

        //WriteLog.Normally("Responding");

        return command.RespondAsync(embed: builder.Build());
    }
}
