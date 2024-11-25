using System;
using System.Reflection;

using PawsyApp.PawsyCore.Modules;

namespace PawsyApp.PawsyCore;
public static class ModuleLoader
{
    internal static IGuildModule? LoadAndInstantiateModule(Assembly from, object[] constructorArgs)
    {

        // Iterate through types in the assembly
        foreach (Type type in from.GetTypes())
        {
            // Check if the type has the "LoadMe" attribute
            if (type.GetCustomAttributes(typeof(PawsyModuleAttribute), true).Length > 0)
            {
                // Check if the type implements IModule
                if (typeof(GuildModule).IsAssignableFrom(type))
                {
                    try
                    {
                        // Create an instance of the type using constructor arguments
                        object? moduleInstance = Activator.CreateInstance(type, constructorArgs);

                        if (moduleInstance is IGuildModule gm)
                        {
                            return gm;
                        }

                        Console.WriteLine($"Unable to load a module from {from.FullName} due to not conforming to module standard or object is null");
                        return null;
                    }
                    catch (MissingMethodException)
                    {
                        Console.WriteLine($"Unable to load a module from {from.FullName} due to missing method");
                        return null;
                    }
                }
            }
        }

        Console.WriteLine($"Skipping {from.FullName} due to no modules present");
        return null;
    }
}
