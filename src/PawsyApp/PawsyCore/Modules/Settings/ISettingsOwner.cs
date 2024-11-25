using System.IO;
using System.Text.Json;

namespace PawsyApp.PawsyCore.Modules.Settings;
public interface ISettingsOwner
{
    string GetSettingsLocation();

    public T LoadSettings<T>() where T : class, ISettings, new()
    {
        FileInfo file = new(GetSettingsLocation());

        if (file.Exists)
        {
            using StreamReader data = new(file.FullName);
            if (JsonSerializer.Deserialize<T>(data.ReadToEnd()) is T Settings)
                return Settings;

        }

        var config = new T();
        config.Save<T>(this);

        return config;
    }
}
