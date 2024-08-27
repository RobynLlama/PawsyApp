using System;
using System.Text;

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
            sb.Append(ContextName);
            sb.Append(": ");
            sb.AppendLine(ContextValue.ToString());
        }

        Normally(sb);
    }

    internal static void Normally(object msg)
    {
        Console.WriteLine($"[Pawsy!] {msg}");
    }
}
