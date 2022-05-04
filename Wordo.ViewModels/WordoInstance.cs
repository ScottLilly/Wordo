using System.Collections.ObjectModel;
using System.ComponentModel;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using Wordo.Core;
using Wordo.Models;

namespace Wordo.ViewModels;

public class WordoInstance : INotifyPropertyChanged
{
    private readonly TaskFactory _uiFactory =
        new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());

    private readonly WordoConfiguration _wordoConfiguration;
    private readonly ConnectionCredentials _credentials;
    private readonly TwitchClient _client = new();

    private string _currentWord = string.Empty;

    public bool IsRunning { get; private set; }
    public ObservableCollection<Letter> Letters { get; } =
        new ObservableCollection<Letter>();
    public ObservableCollection<string> GuessedLetters { get; } =
        new ObservableCollection<string>();

    public event PropertyChangedEventHandler? PropertyChanged;

    public WordoInstance(WordoConfiguration wordoConfiguration)
    {
        _wordoConfiguration = wordoConfiguration;

        _credentials =
            new ConnectionCredentials(
                string.IsNullOrWhiteSpace(wordoConfiguration.BotAccountName)
                    ? wordoConfiguration.ChannelName
                    : wordoConfiguration.BotAccountName,
                wordoConfiguration.TwitchToken,
                disableUsernameCheck: true);

        _client.OnDisconnected += HandleDisconnected;
        _client.OnChatCommandReceived += OnChatCommandReceived;
        _client.OnMessageReceived += OnMessageReceived;

        Connect();

        IsRunning = false;
    }

    private void OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        var messageWords = e.ChatMessage.Message.Split(' ');

        // Wordo will only handle single letter, or single word, messages
        if (!IsRunning || messageWords.Length != 1)
        {
            return;
        }

        string value = messageWords[0];

        if (value.Length == 1)
        {
            // Guess a letter
            if (!GuessedLetters.Any(gl => gl.Matches(value)))
            {
                _uiFactory.StartNew(() => GuessedLetters.Add(value.ToUpper()));

                foreach (Letter letter in Letters)
                {
                    letter.MatchWith(value);
                }
            }
        }
        else
        {
            // Guess a word
            if (value.Matches(_currentWord))
            {
                SendChatMessage($"{e.ChatMessage.DisplayName} correctly guessed the word was '{_currentWord}'");

                StartNewGame();
            }
        }
    }

    private void OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
    {
        if (IsNotFromBroadcasterOrModerator(e.Command.ChatMessage) ||
            e.Command.CommandText.DoesNotMatch("wordo") ||
            e.Command.ArgumentsAsList.None())
        {
            return;
        }

        string wordoArgument = e.Command.ArgumentsAsList[0];

        if (wordoArgument.Matches("start") ||
            wordoArgument.Matches("restart") ||
            wordoArgument.Matches("play"))
        {
            StartNewGame();
        }
        else if (wordoArgument.Matches("stop"))
        {
            StopPlaying();
        }
    }

    private void StartNewGame()
    {
        _currentWord = _wordoConfiguration.Words.RandomElement();

        _uiFactory.StartNew(() => Letters.Clear());
        _uiFactory.StartNew(() => GuessedLetters.Clear());

        foreach (char c in _currentWord.ToList())
        {
            _uiFactory.StartNew(() => Letters.Add(new Letter(c.ToString().ToUpperInvariant())));
        }

        _uiFactory.StartNew(() => IsRunning = true);
    }

    private void StopPlaying()
    {
        _uiFactory.StartNew(() => IsRunning = false);

        _currentWord = "";
        _uiFactory.StartNew(() => Letters.Clear());
        _uiFactory.StartNew(() => GuessedLetters.Clear());
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

    private void SendChatMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _client.SendMessage(_wordoConfiguration.ChannelName, message);
    }

    private static bool IsNotFromBroadcasterOrModerator(ChatMessage chatMessage) =>
        !chatMessage.IsBroadcaster && !chatMessage.IsModerator;
}