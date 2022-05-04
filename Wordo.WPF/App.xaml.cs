using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wordo.WPF;

public partial class App : Application
{
    private string _userSecretsTwitchToken = "";

    private IServiceProvider ServiceProvider { get; set; }
    private IConfiguration Configuration { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets<MainWindow>();

        Configuration = builder.Build();

        base.OnStartup(e);

        // Get token from user secrets (for development)
        _userSecretsTwitchToken =
            Configuration.AsEnumerable().First(c => c.Key == "TwitchToken").Value;

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        // Startup window
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.ShowDialog();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(s => new MainWindow(_userSecretsTwitchToken));
    }
}