using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MuncherLib.Muncher;
using MuncherModule.Settings;
using PawsyApp.PawsyCore.Modules;

namespace MuncherModule;

[PawsyModule(ModuleName)]
public class LogMuncherModule : GuildModule
{
    public const string ModuleName = "log-muncher";
    protected LogMuncherSettings Settings;
    protected static bool RulesInit = false;

    public LogMuncherModule(Guild Owner) : base(Owner, ModuleName, declaresConfig: true)
    {
        Settings = (this as ISettingsOwner).LoadSettings<LogMuncherSettings>();

        if (!RulesInit)
        {
            MuncherLib.RuleDatabase.Rules.Init();
            RulesInit = true;
        }
    }

    public override void OnActivate()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            owner.OnGuildMessage += MessageResponse;
        }
    }

    public override void OnDeactivate()
    {
        if (Owner.TryGetTarget(out var owner))
        {
            owner.OnGuildMessage -= MessageResponse;
        }
    }

    public override void OnConfigDeclared(SlashCommandOptionBuilder rootConfig)
    {
        rootConfig
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Channel)
            .WithName("muncher-channel")
            .WithDescription("The channel Pawsy should watch for logs in")
        );
    }

    public override async Task OnConfigUpdated(SocketSlashCommand command, SocketSlashCommandDataOption options)
    {
        if (Settings is null)
        {
            await command.RespondAsync("Config is unavailable in HandleConfig", ephemeral: true);
            return;
        }

        var option = options.Options.First();
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

                Settings.MunchingChannel = optionChannel.Id;
                (Settings as ISettings).Save<LogMuncherSettings>(this);
                await command.RespondAsync($"Set search channel to <#{optionChannel.Id}>");
                return;
            default:
                await command.RespondAsync("Something went wrong in HandleConfig", ephemeral: true);
                return;
        }
    }

    private async Task MessageResponse(SocketUserMessage message, SocketGuildChannel channel)
    {

        if (Settings is null)
            return;

        if (channel.Id != Settings.MunchingChannel)
            return;

        await LogAppendLine("Checking for attachments");

        foreach (var item in message.Attachments)
        {
            List<Task<string?>> tasks = [];

            if (item.Filename.EndsWith(".log") || item.Filename.EndsWith(".txt"))
            {
                tasks.Add(GetResourceAsync(item.Url));
            }

            //await LogAppendLine("Waiting for downloads...");
            await Task.WhenAll(tasks);
            //await LogAppendLine("done downloading");

            foreach (var data in tasks)
            {
                if (data.Result is null)
                    continue;

                await LogAppendLine("Muncher called");

                StreamReader reader = new(new MemoryStream(Encoding.UTF8.GetBytes(data.Result)));

                //await LogAppendLine("Making a new muncher");

                var munch = new LogMuncher(reader, null!, [], false, false);
                List<LineData> lines;

                await LogAppendLine("Processing output");
                try
                {
                    lines = await munch.MunchLog(true);
                }
                catch (Exception ex)
                {
                    await LogAppendLine($"Exception Happened: {ex}");
                    return;
                }

                await LogAppendLine("Sending results");
                await message.Channel.SendMessageAsync("I see you posted a log file, let me find the most serious issue for you!");

                var items = lines.Count;
                bool DidAny = false;

                if (items != 0)
                {
                    var topIssue = lines[0];
                    lines.Remove(topIssue);

                    await Task.Delay(750);
                    //await LogAppendLine("Sending a result");

                    string output = topIssue.ToStringLimited(500);

                    try
                    {
                        await message.Channel.SendMessageAsync(output, flags: MessageFlags.SuppressEmbeds);
                    }
                    catch (Exception ex)
                    {
                        await LogAppendLine($"Exception Happened: {ex}");
                        return;
                    }

                    DidAny = true;
                }

                //await LogAppendLine("Done with results");

                await Task.Delay(750);
                if (items > 0)
                {
                    var moreIssues = lines.Take(9);
                    var count = moreIssues.Count();

                    using MemoryStream ms = new();
                    using TextWriter writer = new StreamWriter(ms);

                    //allocate 1kb for each line item
                    foreach (var thing in moreIssues)
                    {
                        int take = Math.Min(thing.Contents.Length, 1000);

                        writer.Write($$"""
                        --- ISSUE ---
                        Line Number #{{thing.Line}}
                        Source: {{thing.Source}}
                        Severity: {{thing.Level}} ({{thing.Weight}})

                        Contents:

                        """);

                        writer.Write(thing.Contents.AsSpan(0, take));
                        writer.WriteLine();
                        writer.WriteLine();
                    }

                    writer.Flush();
                    var issue = count > 1 ? "issues" : "issue";
                    await message.Channel.SendFileAsync(ms, "Issues.md", $"I've sorted {count} more {issue} for you");
                }

                await Task.Delay(750);
                if (!DidAny)
                    await message.Channel.SendMessageAsync("I couldn't find any relevant issues in your log file, sorry :sob:");


                //await LogAppendLine("Done!");
            }
        }
    }

    protected async Task<string?> GetResourceAsync(string uri)
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
            await LogAppendLine("Failed to fetch resource");
            return null;
        }
    }
}
