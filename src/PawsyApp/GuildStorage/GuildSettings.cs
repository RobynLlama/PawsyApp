using System.Collections.Generic;

namespace PawsyApp.GuildStorage;

public class GuildSettings
{
    public ulong LoggingChannelID { get; set; } = 0;
    public List<RuleBundle> rules { get; set; } = [];
    public GuildSettings(ulong LoggingChannelID, List<RuleBundle> rules)
    {
        this.LoggingChannelID = LoggingChannelID;
        this.rules = rules;
    }
}
