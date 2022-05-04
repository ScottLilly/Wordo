using System.Collections.ObjectModel;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using Wordo.Core;
using Wordo.Models;

namespace Wordo.ViewModels;

public class WordoInstance
{
    private readonly WordoConfiguration _wordoConfiguration;
    private readonly ConnectionCredentials _credentials;
    private readonly TwitchClient _client = new();

    public ObservableCollection<Letter> Letters { get; } =
        new ObservableCollection<Letter>();

    public WordoInstance(WordoConfiguration wordoConfiguration)
    {
        _wordoConfiguration = wordoConfiguration;

        _credentials =
            new ConnectionCredentials(
                string.IsNullOrWhiteSpace(wordoConfiguration.BotAccountName)
                    ? wordoConfiguration.ChannelName
                    : wordoConfiguration.BotAccountName, wordoConfiguration.TwitchToken, disableUsernameCheck: true);

        _client.OnDisconnected += HandleDisconnected;
        _client.OnChatCommandReceived += OnChatCommandReceived;

        Connect();
    }

    private void OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
    {
        // Only handle "!wordo" commands that include a parameter
        if (!e.Command.CommandText.Matches("wordo") ||
            !e.Command.ArgumentsAsList.Any())
        {
            return;
        }

        string wordoArgument = e.Command.ArgumentsAsList[0];

        if (e.Command.ChatMessage.IsBroadcaster ||
            e.Command.ChatMessage.IsModerator)
        {
            // Handle game start/restart/etc. commands
            if (wordoArgument.Matches("start") ||
                wordoArgument.Matches("restart"))
            {
                SetWord();
            }
        }

        
    }

    private void SetWord()
    {
        string word = _wordoConfiguration.Words.RandomElement();

        Letters.Clear();

        foreach (char c in word.ToList())
        {
            Letters.Add(new Letter(c.ToString().ToUpperInvariant()));
        }
    }

    private void Connect()
    {
        _client.Initialize(_credentials, _wordoConfiguration.ChannelName);
        _client.Connect();
    }

    private void HandleDisconnected(object? sender, OnDisconnectedEventArgs e)
    {
        // If disconnected, automatically attempt to reconnect
        Connect();
    }
}