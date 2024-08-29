using System.Text.Json;

namespace PawsyApp.PawsyCore.Modules.Settings;

internal interface IModuleSettings
{
    internal static readonly JsonSerializerOptions options = new() { WriteIndented = true };
    public string Location { get; set; }
    public IModule? Owner { get; set; }
}
