using System;
using System.Reflection;

using PawsyApp.PawsyCore.Modules;

namespace PawsyApp.PawsyCore;
public static class ModuleLoader
{
    internal static bool TryLoadModuleType(Assembly from, out (string Name, Type type) output)
    {

        // Iterate through types in the assembly
        foreach (Type type in from.GetTypes())
        {
            // Check if the type has the "LoadMe" attribute
            var attrib = type.GetCustomAttributes(typeof(PawsyModuleAttribute), true);

            if (attrib.Length > 0)
            {

                if (attrib[0] is PawsyModuleAttribute Meta)
                {
                    // Check if the type implements IModule
                    if (typeof(GuildModule).IsAssignableFrom(type))
                    {
                        output = (Meta.ModuleName, type);
                        return true;
                    }
                }
            }
        }

        Console.WriteLine($"Skipping {from.FullName} due to no modules present");
        output = (null, null)!;
        return false;
    }
}
