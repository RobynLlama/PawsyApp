using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using PawsyApp.PawsyCore.Modules.Settings;
using PawsyApp.Utils;

namespace PawsyApp.PawsyCore.Modules;

internal interface IModule
{
    IModule? Owner { get; set; }
    string Name { get; }
    ConcurrentBag<IModule> Modules { get; }
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
    public abstract string GetSettingsLocation();
    public T LoadSettings<T>() where T : class, IModuleSettings, new()
    {
        FileInfo file = new(GetSettingsLocation());

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

        WriteLog.Cutely("Failed to read settings", [
            ("Module ", Name),
            ("Settings Type", typeof(T))
        ]);
        var x = new T
        {
            Location = file.FullName,
            Owner = this,
        };
        x.Save<T>();
        return x;
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
