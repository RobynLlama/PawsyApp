using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using PawsyApp.PawsyCore.Modules;

namespace PawsyApp.PawsyCore;

public class Pawsy : IModuleIdent
{
    private IModule? _owner;
    IModule? IModule.Owner { get => _owner; set => _owner = value; }
    public List<IModule> Modules => myModules;
    public ulong ID { get => 0; set { return; } }

    public string Name => "PawsyCore";

    private readonly List<IModule> myModules = [];
    void IModule.Activate()
    {
        throw new NotImplementedException();
    }
}
