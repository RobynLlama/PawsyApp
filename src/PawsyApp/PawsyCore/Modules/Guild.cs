using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PawsyApp.GuildStorage;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules;

internal class GuildModule() : IModuleIdent
{
    private IModule? _owner;
    private readonly List<IModule> _modules = [];
    private ulong _id;
    public IModule? Owner { get => _owner; set => _owner = value; }
    public List<IModule> Modules => _modules;
    public ulong ID { get => _id; set => _id = value; }
    public GuildSettings? Settings;
    void IModule.Activate()
    {
        FileInfo file = new(GuildFile.Get(ID));

        WriteLog.Normally($"Reading settings for guild");

        if (file.Exists)
        {
            using StreamReader data = new(file.FullName);
            if (JsonSerializer.Deserialize<GuildSettings>(data.ReadToEnd()) is GuildSettings Settings)
                this.Settings = Settings;
            return;
        }

        WriteLog.Normally("Failed to read settings");
        Settings = new(ID);
    }
}
