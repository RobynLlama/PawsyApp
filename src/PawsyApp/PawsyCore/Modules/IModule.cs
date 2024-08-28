using System.Collections.Generic;

namespace PawsyApp.PawsyCore.Modules;

internal interface IModule
{
    IModule? Owner { get; set; }
    List<IModule> Modules { get; }
    void AddModule<T>() where T : IModule, new()
    {
        T item = new()
        {
            Owner = this
        };

        Modules.Add(item);
    }
    T? GetModule<T>() where T : class, IModule
    {
        foreach (var item in Modules)
        {
            if (item is T thing)
                return thing;
        }

        return null;
    }
    void RemoveModule(IModule module)
    {
        Modules.Remove(module);
    }
    void Destroy()
    {
        Owner?.RemoveModule(this);
    }
}
