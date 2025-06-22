using integration_dispatch.Interfaces;
using integration_dispatch.Model;
using Newtonsoft.Json;

namespace integration_dispatch.Utils
{
    public class ConfigLoader : IConfigLoader
    {
        private readonly string _configPath;

        public ConfigLoader(string configPath = "config.json")
        {
            _configPath = configPath;
        }

        public async Task<Config> LoadConfigAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_configPath);
                return JsonConvert.DeserializeObject<Config>(json) ?? throw new InvalidOperationException("Configuration file is empty or invalid.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load configuration: {ex.Message}", ex);
            }
        }
    }
}
