using System.Windows;
using Wordo.Models;
using Wordo.Services;
using Wordo.ViewModels;

namespace Wordo.WPF;

public partial class MainWindow : Window
{
    private WordoInstance VM => DataContext as WordoInstance;

    public MainWindow(string userSecretsToken)
    {
        InitializeComponent();

        WordoConfiguration wordoConfiguration =
            PersistenceService.GetWordoConfiguration(userSecretsToken);

        DataContext = new WordoInstance(wordoConfiguration);

        VM.PropertyChanged += VM_PropertyChanged;
    }

    private void VM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName.Equals(nameof(WordoInstance.IsRunning)))
        {
            Visibility = VM.IsRunning ? Visibility.Visible : Visibility.Hidden;
        }
    }
}