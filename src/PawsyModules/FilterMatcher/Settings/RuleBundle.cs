using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Discord.WebSocket;

namespace FilterMatcher.Settings;

//new( new("""smelly""", RegexOptions.IgnoreCase | RegexOptions.Compiled, new(0, 0, 1)), "Test Rule")
public class RuleBundle
{
    internal Regex? reg;
    [JsonInclude]
    public string Regex { get; set; }
    public string? ResponseMSG { get; set; }
    public string RuleName { get; set; }

    public int WarnColorRed { get; set; } = 255;
    public int WarnColorGreen { get; set; } = 0;
    public int WarnColorBlue { get; set; } = 0;
    public bool DeleteMessage { get; set; } = false;
    public bool WarnStaff { get; set; } = true;
    public FilterType FilterStyle { get; set; } = FilterType.WhiteList;
    public List<ulong> FilteredChannels { get; set; } = [];
    public int Cooldown { get; set; } = 0;

    [JsonIgnore]
    public long lastMatchTime { get; set; }
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

    public RuleBundle(string Regex, string RuleName, string ResponseMSG) : this(Regex, RuleName)
    {
        this.ResponseMSG = ResponseMSG;
        SendResponse = true;
    }

    [JsonConstructor]
    public RuleBundle(string Regex, string RuleName)
    {
        this.Regex = Regex;
        this.RuleName = RuleName;
    }

    public bool Match(string content, SocketGuildChannel channel)
    {
        

        var channelID = channel.Id;

        if (channel is SocketThreadChannel tChannel)
        {
            //WriteLog.Normally("Filter is applied inside a thread");
            channelID = tChannel.ParentChannel.Id;
        }

        var isOnList = FilteredChannels.Contains(channelID);

        if ((FilterStyle == FilterType.BlackList && isOnList) || (FilterStyle == FilterType.WhiteList && !isOnList))
            return false;

        reg ??= new(Regex, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline, new(0, 0, 1));
        return reg.Match(content).Success;
    }

    public static bool isValid(string regex)
    {
        if (string.IsNullOrWhiteSpace(regex)) return false;

        try
        {
            System.Text.RegularExpressions.Regex.IsMatch("", regex);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
