using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules;

internal interface IModule
{
    IModule? Owner { get; set; }
    string Name { get; }
    List<IModule> Modules { get; }
    IModuleSettings? Settings { get; }
    public T AddModule<T>() where T : IModule, new()
    {
        T item = new()
        {
            Owner = this
        };

        item.Activate();

        Modules.Add(item);
        return item;
    }
    public T? GetModule<T>() where T : class, IModule
    {
        foreach (var item in Modules)
        {
            if (item is T thing)
                return thing;
        }

        return null;
    }
    public void RemoveModule(IModule module)
    {
        Modules.Remove(module);
    }
    public void Destroy()
    {
        Owner?.RemoveModule(this);
    }

    public string GetSettingsLocation();
    public T LoadSettings<T>() where T : class, IModuleSettings, new()
    {
        FileInfo file = new(GetSettingsLocation());

        WriteLog.Normally($"Reading settings");

        if (file.Exists)
        {
            using StreamReader data = new(file.FullName);
            if (JsonSerializer.Deserialize<T>(data.ReadToEnd()) is T Settings)
            {
                Settings.Location = file.FullName;
                Settings.Owner = this;
                return Settings;
            }

        }

        WriteLog.Normally("Failed to read settings");
        return new();
    }

    public T? GetSettings<T>() where T : class, IModuleSettings
    {
        if (Settings is T outSettings)
        {
            return outSettings;
        }

        return null;
    }

    abstract void Activate();
}
