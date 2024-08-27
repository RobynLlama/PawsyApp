using System;
using System.Text;
using PawsyApp.KittyColors;

namespace PawsyApp.Utils;

internal class WriteLog
{
    internal static void Cutely(object msg, (object ContextName, object ContextValue)[] context)
    {
        StringBuilder sb = new(msg.ToString());
        sb.AppendLine();

        foreach (var (ContextName, ContextValue) in context)
        {
            sb.Append("  ");
            sb.Append(KittyColor.WrapInColor(ContextName.ToString(), ColorCode.Cyan));
            sb.Append(": ");
            sb.AppendLine(ContextValue.ToString());
        }

        Normally(sb);
    }

    internal static void Normally(object msg)
    {
        Console.WriteLine($"[{KittyColor.WrapInColor("Pawsy!", ColorCode.Magenta)}] {msg}");
    }
}
