using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace integration_dispatch.Utils
{
    public class LastRunManager
    {
        private readonly string _filePath;

        public LastRunManager(string filePath = "lastRun.json")
        {
            _filePath = filePath;
        }

        public async Task<DateTime?> LoadLastRunDateAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return null;

                var json = await File.ReadAllTextAsync(_filePath);
                return JsonConvert.DeserializeObject<DateTime>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load last run date: {ex.Message}", ex);
            }
        }

        public async Task SaveLastRunDateAsync(DateTime date)
        {
            try
            {
                var json = JsonConvert.SerializeObject(date);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save last run date: {ex.Message}", ex);
            }
        }
    }
}