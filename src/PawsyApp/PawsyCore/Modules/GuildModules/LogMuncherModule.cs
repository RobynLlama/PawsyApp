using PawsyApp.PawsyCore.Modules.Settings;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using MuncherLib.Muncher;
using System.Linq;
using Discord;

namespace PawsyApp.PawsyCore.Modules.GuildModules;

internal class LogMuncherModule : GuildModule
{
    protected LogMuncherSettings Settings;
    protected static bool RulesInit = false;

    public LogMuncherModule(Guild Owner) : base(Owner, "log-muncher", declaresConfig: true)
    {
        Settings = (this as ISettingsOwner).LoadSettings<LogMuncherSettings>();

        if (!RulesInit)
        {
            MuncherLib.RuleDatabase.Rules.Init();
            RulesInit = true;
        }
    }

    public override void OnModuleActivation()
    {
        Owner.OnGuildMessage += MessageResponse;
    }

    public override void OnModuleDeactivation()
    {
        Owner.OnGuildMessage -= MessageResponse;
    }

    public override void OnModuleDeclareConfig(SlashCommandOptionBuilder rootConfig)
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

        await Owner.Pawsy.LogAppendLine(Name, "Checking for attachments");

        foreach (var item in message.Attachments)
        {
            List<Task<string?>> tasks = [];

            if (item.Filename.EndsWith(".log") || item.Filename.EndsWith(".txt"))
            {
                tasks.Add(GetResourceAsync(item.Url));
            }

            //await Owner.Pawsy.LogAppendLine(Name, "Waiting for downloads...");
            await Task.WhenAll(tasks);
            //await Owner.Pawsy.LogAppendLine(Name, "done downloading");

            foreach (var data in tasks)
            {
                if (data.Result is null)
                    continue;

                await Owner.Pawsy.LogAppendLine(Name, "Muncher called");

                StreamReader reader = new(new MemoryStream(Encoding.UTF8.GetBytes(data.Result)));

                //await Owner.Pawsy.LogAppendLine(Name, "Making a new muncher");

                var munch = new LogMuncher(reader, null!, [], false, false);

                //await Owner.Pawsy.LogAppendLine(Name, "Processing output");
                var lines = await munch.MunchLog(true);

                //await Owner.Pawsy.LogAppendLine(Name, "Sending results");
                await message.Channel.SendMessageAsync("I see you posted a log file, let me find the most serious errors for you..");

                var issues = lines.Take(2);
                bool DidAny = false;

                foreach (var thing in issues)
                {
                    await Task.Delay(750);
                    //await Owner.Pawsy.LogAppendLine(Name, "Sending a result");
                    await message.Channel.SendMessageAsync(thing.ToString(), flags: Discord.MessageFlags.SuppressEmbeds);
                    DidAny = true;
                }

                //await Owner.Pawsy.LogAppendLine(Name, "Done with results");

                await Task.Delay(750);
                if (DidAny)
                {
                    await message.Channel.SendMessageAsync("I hope this helps");
                }
                else
                {
                    await message.Channel.SendMessageAsync("I couldn't find any relevant issues in your log file, sorry :sob:");
                }


                //await Owner.Pawsy.LogAppendLine(Name, "Done!");
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
            await Owner.Pawsy.LogAppendLine(Name, "Failed to fetch resource");
            return null;
        }
    }
}
