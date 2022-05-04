using Wordo.Models;

namespace Wordo.ViewModels;

public class WordoInstance
{
    private readonly WordoConfiguration _wordoConfiguration;

    public WordoInstance(WordoConfiguration wordoConfiguration)
    {
        _wordoConfiguration = wordoConfiguration;
    }
}