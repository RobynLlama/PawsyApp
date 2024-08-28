using System.Collections.Generic;
using PawsyApp.PawsyCore.Modules;

namespace PawsyApp.PawsyCore;

internal class Pawsy : IModule
{
    IModule? IModule.Owner { get => _owner; set => _owner = value; }
    public List<IModule> Modules => myModules;
    private IModule? _owner = null;
    private readonly List<IModule> myModules = [];

    public Pawsy()
    {
        //Add submodules
    }
}
