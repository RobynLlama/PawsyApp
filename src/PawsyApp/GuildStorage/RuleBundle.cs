using System.Text.RegularExpressions;

namespace PawsyApp.GuildStorage;

//new( new("""smelly""", RegexOptions.IgnoreCase | RegexOptions.Compiled, new(0, 0, 1)), "Test Rule")
public class RuleBundle
{
    internal Regex? reg;
    public string regex { get; set; }
    public string? response { get; set; }
    public string name { get; set; }
    public int color_R { get; set; } = 255;
    public int color_G { get; set; } = 0;
    public int color_B { get; set; } = 0;
    public bool delete_message { get; set; } = false;
    public bool warn_staff { get; set; } = true;
    public bool send_response { get; set; } = false;

    public RuleBundle(string regex, string name, string response, int color_R, int color_G, int color_B, bool delete_message, bool warn_staff, bool send_response)
    {
        this.regex = regex;
        this.response = response;
        this.name = name;
        this.color_R = color_R;
        this.color_B = color_B;
        this.color_G = color_G;
        this.send_response = send_response;
        this.delete_message = delete_message;
        this.warn_staff = warn_staff;
    }

    public bool Match(string content)
    {
        reg ??= new(regex, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline, new(0, 0, 1));
        return reg.Match(content).Success;
    }
}
