using System.IO;
using System.Text.Json;

namespace PawsyApp.PawsyCore.Modules.Settings;

public interface ISettings
{
    internal static readonly JsonSerializerOptions options = new() { WriteIndented = true };
    void Save<T>(ISettingsOwner owner) where T : class
    {
        if (this is T serial)
        {
            using StreamWriter writer = new(owner.GetSettingsLocation());
            writer.Write(JsonSerializer.Serialize(serial, options));
        }
    }
}
