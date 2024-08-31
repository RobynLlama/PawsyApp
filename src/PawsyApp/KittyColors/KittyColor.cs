namespace PawsyApp.KittyColors;

internal static class KittyColor
{
    internal static string reset = "\x1B[0m";
    internal static string GetPrintableColor(ColorCode code, bool background = false)
    {
        int ansii;

        if (background)
            ansii = 40;
        else
            ansii = 30;

        return $"\x1B[{ansii + (int)code}m";
    }

    internal static string WrapInColor(string? input, ColorCode foreground)
    {
        if (input is null)
            return string.Empty;

        return $"{GetPrintableColor(foreground)}{input}{reset}";
    }
}
