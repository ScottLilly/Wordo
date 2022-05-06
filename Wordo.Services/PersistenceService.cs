using Newtonsoft.Json;
using Wordo.Models;

namespace Wordo.Services
{
    public static class PersistenceService
    {
        private const string WORDO_CONFIGURATION_FILE_NAME = "appsettings.json";
        private const string WORDO_POINTS_DATA_FILE_NAME = "wordopoints.json";

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

        public static WordoPointsData GetWordoPointsData()
        {
            return File.Exists(WORDO_POINTS_DATA_FILE_NAME)
                ? JsonConvert.DeserializeObject<WordoPointsData>(File.ReadAllText(WORDO_POINTS_DATA_FILE_NAME))
                : new WordoPointsData();
        }

        public static void SaveWordoPointsData(WordoPointsData wordoPointsData)
        {
            File.WriteAllText(WORDO_POINTS_DATA_FILE_NAME,
                JsonConvert.SerializeObject(wordoPointsData, Formatting.Indented));
        }
    }
}