using System.Collections.ObjectModel;
using System.ComponentModel;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using Wordo.Core;
using Wordo.Models;
using Wordo.Services;

namespace Wordo.ViewModels;

public class WordoInstance : INotifyPropertyChanged
{
    #region Backing variables

    private readonly TaskFactory _uiFactory =
        new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
    private readonly WordoConfiguration _wordoConfiguration;
    private readonly ConnectionCredentials _credentials;
    private readonly TwitchClient _client = new();
    private readonly WordoPointsData _wordoPointsData;
    private string _currentWord = string.Empty;

    #endregion

    #region Public properties and events

    public bool IsRunning { get; private set; }
    public ObservableCollection<Letter> Letters { get; } =
        new ObservableCollection<Letter>();
    public ObservableCollection<string> GuessedLetters { get; } =
        new ObservableCollection<string>();

    public string LastWord { get; private set; } = "";
    public string LastWinner { get; private set; } = "";

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    public WordoInstance(WordoConfiguration wordoConfiguration)
    {
        _wordoConfiguration = wordoConfiguration;
        _wordoPointsData = PersistenceService.GetWordoPointsData();

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

        string message = messageWords[0];

        // Handle when user message was a single letter that has not already been guessed
        if (message.Length == 1 &&
            char.IsLetter(message[0]) &&
            GuessedLetters.None(gl => gl.Matches(message)))
        {
            _uiFactory.StartNew(() => GuessedLetters.Add(message.ToUpper()));

            foreach (Letter letter in Letters)
            {
                letter.CompareWithGuess(message);
            }
        }

        // Word was guessed, or all letters were guessed
        if (message.Matches(_currentWord) ||
            Letters.All(l => l.WasGuessed))
        {
            HandleWin(e.ChatMessage.UserId, e.ChatMessage.DisplayName, _currentWord);
        }
    }

    private void OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
    {
        if (e.Command.CommandText.DoesNotMatch("wordo"))
        {
            return;
        }

        // Default handling of "!wordo" (without parameters) is to display chatter's Wordo points.
        if (e.Command.ArgumentsAsList.None())
        {
            var points =
                _wordoPointsData.UserPoints
                    .FirstOrDefault(up => up.Id.Equals(e.Command.ChatMessage.UserId))?.Points ?? 0;

            SendChatMessage($"{e.Command.ChatMessage.DisplayName}, you have {points} Wordo points");
        }

        // Handle commands with a parameter (for all users)
        if (e.Command.ArgumentsAsList[0].Matches("top"))
        {
            DisplayTopScores();
        }

        HandleBroadcasterAndModeratorCommands(e.Command);
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

    private void HandleBroadcasterAndModeratorCommands(ChatCommand chatCommand)
    {
        if (IsNotFromBroadcasterOrModerator(chatCommand.ChatMessage) ||
            chatCommand.ArgumentsAsList.None())
        {
            return;
        }

        string wordoArgument = chatCommand.ArgumentsAsList[0];

        if (wordoArgument.Matches("start") ||
            wordoArgument.Matches("play"))
        {
            StartNewGame();
        }
        else if (wordoArgument.Matches("stop"))
        {
            StopPlaying();
        }
    }

    private void DisplayTopScores()
    {
        var topPlayers =
            _wordoPointsData.UserPoints
                .OrderByDescending(up => up.Points)
                .ThenBy(up => up.Name)
                .Take(5);

        SendChatMessage($"Top Scores: {string.Join(' ', topPlayers.Select(tp => $"{tp.Name}: {tp.Points}"))}");
    }

    private void SendChatMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _client.SendMessage(_wordoConfiguration.ChannelName, message);
    }

    private void HandleWin(string winnerUserId, string winnerDisplayName, string word)
    {
        SendChatMessage($"{winnerDisplayName} correctly guessed the word was '{word}'");

        GiveWordoPoints(winnerUserId, winnerDisplayName);

        _uiFactory.StartNew(() => LastWord = word.ToUpper());
        _uiFactory.StartNew(() => LastWinner = winnerDisplayName);

        StartNewGame();
    }

    private void GiveWordoPoints(string userId, string displayName)
    {
        if (_wordoPointsData.UserPoints.None(up => up.Id.Matches(userId)))
        {
            _wordoPointsData.UserPoints.Add(new WordoPointsData.UserPoint
            {
                Id = userId,
                Name = displayName,
                Points = 0
            });
        }

        _wordoPointsData.UserPoints.First(up => up.Id.Matches(userId)).Points += 10;

        PersistenceService.SaveWordoPointsData(_wordoPointsData);
    }

    private static bool IsNotFromBroadcasterOrModerator(ChatMessage chatMessage) =>
        !chatMessage.IsBroadcaster && !chatMessage.IsModerator;
}