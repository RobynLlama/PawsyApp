using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PawsyApp.PawsyCore.Modules.Settings;

internal interface IModuleSettings
{
    [JsonIgnore]
    internal static readonly JsonSerializerOptions options = new() { WriteIndented = true };
    [JsonIgnore]
    public string Location { get; set; }
    [JsonIgnore]
    public IModule? Owner { get; set; }
    void Save<T>() where T : class, IModuleSettings
    {
        if (this is T serial)
        {
            using StreamWriter writer = new(Location);
            writer.Write(JsonSerializer.Serialize(serial, options));
        }
    }
}
