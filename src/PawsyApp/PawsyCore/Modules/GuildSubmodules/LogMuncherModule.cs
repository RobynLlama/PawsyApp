using System.Collections.Concurrent;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;
using LogMuncher;
using PawsyApp.PawsyCore.Modules.Core;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;

namespace PawsyApp.PawsyCore.Modules.GuildSubmodules;

internal class LogMuncherModule : GuildSubmodule
{
    public override IModule? Owner { get => _owner; set => _owner = value; }
    public override string Name => "LogMuncher";
    public override ConcurrentBag<IModule> Modules => _modules;
    public override IModuleSettings? Settings => _settings;

    protected IModule? _owner;
    protected readonly ConcurrentBag<IModule> _modules = [];
    protected FilterMatcherSettings? _settings;

    public override void Activate()
    {
        WriteLog.Normally("Muncher module ready to munch");
    }

    public override void RegisterHooks()
    {
        if (Owner is GuildModule guild)
        {
            guild.OnGuildMessage += MessageResponse;
        }
    }

    private async Task MessageResponse(SocketUserMessage message, SocketGuildChannel channel)
    {

        await WriteLog.Normally("Checking for attachments");

        foreach (var item in message.Attachments)
        {
            List<Task<string?>> tasks = [];

            if (item.Filename.EndsWith(".log"))
            {
                tasks.Add(GetResourceAsync(item.Url));
            }

            await Task.WhenAll(tasks);

            foreach (var data in tasks)
            {
                if (data.Result is null)
                    continue;

                string filename = "./cache/" + data.Result.GetHashCode().ToString();

                FileInfo input = new(filename);
                FileInfo output = new(filename + ".html");

                using (StreamWriter writer = new(input.FullName))
                    writer.Write(data.Result);

                var f = new StreamWriter(output.FullName);

                await WriteLog.Normally($"Writing to {data.GetHashCode()}");
                LogMuncher.Muncher.TheLogMuncher.quiet = true;
                await new LogMuncher.Muncher.TheLogMuncher(input, f, []).MunchLog();
                await WriteLog.Normally($"Output buffer is {f.ToString().Length}");
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
            await WriteLog.Normally("Failed to fetch resource");
            return null;
        }
    }
}
