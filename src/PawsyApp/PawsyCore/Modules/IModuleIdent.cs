namespace PawsyApp.PawsyCore.Modules;

internal interface IModuleIdent : IModule, IUnique<ulong>
{
    internal T AddModuleIdent<T>(ulong ModID) where T : IModuleIdent, new()
    {
        T item = new()
        {
            ID = ModID,
            Owner = this
        };

        (item as IModule).Activate();

        Modules.Add(item);
        return item;
    }
    public T? GetModuleIdent<T>(ulong ModID) where T : class, IModuleIdent
    {
        foreach (var item in Modules)
        {
            if (item is T thing)
                if (thing.ID == ModID)
                    return thing;
        }

        return null;
    }
}
