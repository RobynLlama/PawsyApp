using System.Collections.Generic;

namespace PawsyApp.PawsyCore;

internal interface IUniqueCollection<T>
{
    IEnumerable<IUnique<T>> UniqueCollection { get; }

    IUnique<T>? GetUniqueItem(T ID)
    {
        foreach (var item in UniqueCollection)
        {
            if (item.ID is not null && item.ID.Equals(ID))
                return item;
        }

        return null;
    }

    bool UniqueContains(T ID)
    {
        return GetUniqueItem(ID) is not null;
    }
}
