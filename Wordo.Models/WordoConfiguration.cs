﻿namespace Wordo.Models;

public class WordoConfiguration
{
    public string ChannelName { get; set; }
    public string BotAccountName { get; set; }
    public string TwitchToken { get; set; }
    public string BotDisplayName { get; set; }
    public List<string> Words { get; set; } = new List<string>();
}