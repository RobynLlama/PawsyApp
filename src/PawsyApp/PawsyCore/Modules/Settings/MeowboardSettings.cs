using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules.Settings;

internal class MeowBoardSettings() : ISettings
{
    [JsonInclude]
    internal ConcurrentDictionary<ulong, int> Records { get; set; } = [];

    [JsonInclude]
    internal int MeowBoardDisplayLimit = 5;

    public void AddUserMeow(ulong userID)
    {
        if (Records.TryGetValue(userID, out int amount))
            Records[userID] = amount + 1;
        else
            Records.TryAdd(userID, 1);
    }

    public Task EmbedMeowBoard(SocketSlashCommand command)
    {

        if (Helpers.GetGuild(command.GuildId) is not SocketGuild guild)
        {
            command.RespondAsync("Something went wrong", ephemeral: true);
            return Task.CompletedTask;
        }


        EmbedBuilder builder = new();
        //WriteLog.Normally("MeowBoard being built");

        var top5 = Records.OrderByDescending(kvp => kvp.Value)
                    .Take(MeowBoardDisplayLimit)
                    .ToList();

        static IEnumerable<EmbedFieldBuilder> fields(List<KeyValuePair<ulong, int>> items, SocketGuild guild)
        {
            foreach (var item in items)
            {

                string username;

                if (guild.GetUser(item.Key) is not SocketGuildUser user)
                    continue;
                else
                    username = user.Nickname ?? user.GlobalName ?? user.Username;


                //WriteLog.Normally("Adding a field to the MeowBoard");
                yield return new EmbedFieldBuilder().WithName(username).WithValue(item.Value.ToString());
            }
        }

        builder
            //.WithAuthor("Pawsy!")
            .WithColor(0, 128, 196)
            .WithDescription($"Meow Board top {MeowBoardDisplayLimit}")
            .WithTitle("Meow Board")
            .WithThumbnailUrl("https://raw.githubusercontent.com/RobynLlama/PawsyApp/main/Assets/img/Pawsy-small.png")
            .WithFields(fields(top5, guild))
            .WithUrl("https://github.com/RobynLlama/PawsyApp")
            .WithCurrentTimestamp();

        //WriteLog.Normally("Responding");

        return command.RespondAsync(embed: builder.Build());
    }
}
