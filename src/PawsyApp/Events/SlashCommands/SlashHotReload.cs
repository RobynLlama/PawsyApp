using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PawsyApp.GuildStorage;

namespace PawsyApp.Events.SlashCommands;

internal class SlashHotReload : ISlashCommand
{
    public SlashCommandHandler.SlashHandler Handler => HandleCommand;

    private static async Task HandleCommand(SocketSlashCommand command)
    {
        List<Task> tasks = [];

        if (command.GuildId is ulong realID)
        {
            tasks.Add(command.RespondAsync($"Hot reloading this guild, meow!"));
            Settings.AllSettings.GuildSettingsStorage.Remove(realID, out _);

            if (PawsyProgram._client?.GetGuild(realID) is SocketGuild guild)
                tasks.Add(GuildAvailable.Respond(guild));
        }
        else
        {
            tasks.Add(command.RespondAsync());
        }

        await Task.WhenAll(tasks);
        return;
    }

    public SlashCommandProperties BuiltCommand => MyCommand;

    public string ModuleName => "debug";

    private static SlashCommandProperties MyCommand = new SlashCommandBuilder().WithName("hot-reload").WithDescription("Reload this guild's configuration").WithDefaultMemberPermissions(GuildPermission.ManageGuild).Build();

}
