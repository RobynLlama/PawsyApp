using System;

namespace PawsyApp.PawsyCore.Modules;

[AttributeUsage(AttributeTargets.Class)]
public class PawsyModuleAttribute(string ModuleName) : Attribute
{
  public string ModuleName = ModuleName;
}
