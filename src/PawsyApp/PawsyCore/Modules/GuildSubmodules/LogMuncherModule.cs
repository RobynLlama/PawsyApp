using System.Collections.Concurrent;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;
using System.Text;
using PawsyApp.PawsyCore.Modules.Core;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using MuncherLib.Muncher;
using System.Linq;
using Discord;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

internal class LogMuncherModule : GuildSubmodule
{
    public override string Name => "log-muncher";
    public override IModuleSettings? Settings => _settings;

    protected LogMuncherSettings? _settings;
    protected static bool RulesInit = false;

    public override void Alive()
    {
        WriteLog.LineNormal("Muncher module ready to munch");
        _settings = (this as IModule).LoadSettings<LogMuncherSettings>();

        if (!RulesInit)
        {
            MuncherLib.RuleDatabase.Rules.Init();
            RulesInit = true;
        }

        var ConfigCommand = new SlashCommandBuilder()
        .WithName("muncher-config")
        .WithDescription("Configure the Log Muncher module")
        .WithDefaultMemberPermissions(GuildPermission.ManageGuild)
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Channel)
            .WithName("muncher-channel")
            .WithDescription("The channel Pawsy will look for logs in")
        );

        if (Owner is GuildModule guild)
        {
            guild.RegisterSlashCommand(
                new(HandleConfig, ConfigCommand.Build(), Name)
            );
        }
    }

    public override void OnModuleActivation()
    {
        if (Owner is GuildModule guild)
        {
            guild.OnGuildMessage += MessageResponse;
        }
    }

    public override void OnModuleDeactivation()
    {
        if (Owner is GuildModule guild)
        {
            guild.OnGuildMessage -= MessageResponse;
        }
    }

    private async Task HandleConfig(SocketSlashCommand command)
    {

        if (_settings is null)
        {
            await command.RespondAsync("Config is unavailable in HandleConfig", ephemeral: true);
            return;
        }

        var option = command.Data.Options.First();
        var optionName = option.Name;
        var optionValue = option.Value;

        switch (optionName)
        {
            case "muncher-channel":
                if (optionValue is not SocketTextChannel optionChannel)
                {
                    await command.RespondAsync("Only text channels, please and thank mew", ephemeral: true);
                    return;
                }

                _settings.MunchingChannel = optionChannel.Id;
                (_settings as IModuleSettings).Save<LogMuncherSettings>();
                await command.RespondAsync($"Set search channel to <#{optionChannel.Id}>");
                return;
            default:
                await command.RespondAsync("Something went wrong in HandleConfig", ephemeral: true);
                return;
        }
    }

    private async Task MessageResponse(SocketUserMessage message, SocketGuildChannel channel)
    {

        if (_settings is null)
            return;

        if (channel.Id != _settings.MunchingChannel)
            return;

        await WriteLog.LineNormal("Checking for attachments");

        foreach (var item in message.Attachments)
        {
            List<Task<string?>> tasks = [];

            if (item.Filename.EndsWith(".log") || item.Filename.EndsWith(".txt"))
            {
                tasks.Add(GetResourceAsync(item.Url));
            }

            await WriteLog.LineNormal("Waiting for downloads...");
            await Task.WhenAll(tasks);
            await WriteLog.LineNormal("done downloading");

            foreach (var data in tasks)
            {
                if (data.Result is null)
                    continue;

                //await WriteLog.Normally("Making a stream for an attachment");

                StreamReader reader = new(new MemoryStream(Encoding.UTF8.GetBytes(data.Result)));
                //StreamWriter writer = new("");

                //await WriteLog.Normally("Making a new muncher");

                var munch = new LogMuncher(reader, null!, [], false, false);

                //await WriteLog.Normally("Processing output");
                var lines = await munch.MunchLog(true);

                //await WriteLog.Normally("Sending results");
                await message.Channel.SendMessageAsync("I see you posted a log file, let me find the most serious errors for you..");

                var issues = lines.Take(2);
                bool DidAny = false;

                foreach (var thing in issues)
                {
                    await Task.Delay(750);
                    await message.Channel.SendMessageAsync(thing.ToString(), flags: Discord.MessageFlags.SuppressEmbeds);
                    DidAny = true;
                }

                await Task.Delay(750);
                if (DidAny)
                {
                    await message.Channel.SendMessageAsync("I hope this helps");
                }
                else
                {
                    await message.Channel.SendMessageAsync("I couldn't find any relevant issues in your log file, sorry :sob:");
                }


                await WriteLog.LineNormal("Done!");
            }
        }
    }

    protected static async Task<string?> GetResourceAsync(string uri)
    {
        using HttpClient client = new();
        try
        {
            // Send a GET request to the specified URI.
            HttpResponseMessage response = await client.GetAsync(uri);

            // Ensure the request was successful.
            response.EnsureSuccessStatusCode();

            // Read the response content as a string.
            string content = await response.Content.ReadAsStringAsync();
            return content;
        }
        catch (HttpRequestException)
        {
            // Handle any exceptions that occur during the request.
            await WriteLog.LineNormal("Failed to fetch resource");
            return null;
        }
    }
}
