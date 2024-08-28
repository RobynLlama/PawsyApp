using System.Collections.Generic;

namespace PawsyApp.PawsyCore.Modules;

public interface IModule
{
    IModule? Owner { get; set; }
    List<IModule> Modules { get; }
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

    abstract void Activate();
}
