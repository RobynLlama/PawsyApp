using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace PawsyApp.GuildStorage;

//new( new("""smelly""", RegexOptions.IgnoreCase | RegexOptions.Compiled, new(0, 0, 1)), "Test Rule")
public class RuleBundle
{
    internal Regex? reg;
    public string Regex { get; set; }
    public string? ResponseMSG { get; set; }
    public string RuleName { get; set; }
    public int ColorR { get; set; } = 255;
    public int ColorG { get; set; } = 0;
    public int ColorB { get; set; } = 0;
    public bool DeleteMessage { get; set; } = false;
    public bool WarnStaff { get; set; } = true;
    public bool SendResponse
    {
        get
        {
            if (ResponseMSG is null)
                return false;

            return _sendResponse;
        }
        set
        {
            _sendResponse = value;
        }
    }
    protected bool _sendResponse = false;

    public RuleBundle(string Regex, string RuleName, string ResponseMSG)
    {
        this.Regex = Regex;
        this.ResponseMSG = ResponseMSG;
        this.RuleName = RuleName;
        SendResponse = true;
    }

    [JsonConstructor]
    public RuleBundle(string Regex, string RuleName)
    {
        this.Regex = Regex;
        this.RuleName = RuleName;
    }

    public bool Match(string content)
    {
        reg ??= new(Regex, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline, new(0, 0, 1));
        return reg.Match(content).Success;
    }
}
