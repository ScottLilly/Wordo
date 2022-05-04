using System.Windows;
using Wordo.Models;
using Wordo.Services;
using Wordo.ViewModels;

namespace Wordo.WPF;

public partial class MainWindow : Window
{
    public MainWindow(string userSecretsToken)
    {
        InitializeComponent();

        WordoConfiguration wordoConfiguration =
            PersistenceService.GetWordoConfiguration(userSecretsToken);

        DataContext = new WordoInstance(wordoConfiguration);
    }
}