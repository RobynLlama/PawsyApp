using System.Collections.Generic;

namespace PawsyApp.PawsyCore;

internal interface IUniqueCollection<T, W> where W : class, IUnique<T>
{
    IEnumerable<W> UniqueCollection { get; }

    W? GetUniqueItem(T ID)
    {
        foreach (var item in UniqueCollection)
        {
            if (item.ID is not null && item.ID.Equals(ID))
                return item;
        }

        return null;
    }

    bool TryGetUniqueItem(T ID, out W? item)
    {
        item = GetUniqueItem(ID);
        return item is not null;
    }
}
