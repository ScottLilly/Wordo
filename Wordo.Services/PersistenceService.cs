using Newtonsoft.Json;
using Wordo.Models;

namespace Wordo.Services
{
    public static class PersistenceService
    {
        private const string WORDO_CONFIGURATION_FILE_NAME = "appsettings.json";

        public static WordoConfiguration GetWordoConfiguration(string userSecretsToken)
        {
            WordoConfiguration wordoConfiguration =
                File.Exists(WORDO_CONFIGURATION_FILE_NAME)
                    ? JsonConvert.DeserializeObject<WordoConfiguration>(File.ReadAllText(WORDO_CONFIGURATION_FILE_NAME))
                    : new WordoConfiguration();

            if (string.IsNullOrWhiteSpace(wordoConfiguration.TwitchToken))
            {
                wordoConfiguration.TwitchToken = userSecretsToken;
            }

            return wordoConfiguration;
        }
    }
}