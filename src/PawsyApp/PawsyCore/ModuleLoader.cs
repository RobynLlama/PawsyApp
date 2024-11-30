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

    internal static bool TryInstantiateModuleFromType(Type from, object[] constructorArgs, out IGuildModule output)
    {

        // Check if the type implements IModule
        if (typeof(GuildModule).IsAssignableFrom(from))
        {
            try
            {
                // Create an instance of the type using constructor arguments
                object? moduleInstance = Activator.CreateInstance(from, constructorArgs);

                if (moduleInstance is IGuildModule gm)
                {
                    output = gm;
                    return true;
                }

                Console.WriteLine($"Unable to instance a module from {from.Name} due to not conforming to module standard or object is null");
                output = null!;
                return false;
            }
            catch (MissingMethodException)
            {
                Console.WriteLine($"Unable to instance a module from {from.Name} due to missing method");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while attempting to instance {from.Name}\nStack: {ex}");
            }
            finally
            {
                output = null!;
            }

            return false;
        }

        Console.WriteLine($"Unable to instance a module from {from.Name} due to not conforming to module standard or object is null");
        output = null!;
        return false;
    }
}
