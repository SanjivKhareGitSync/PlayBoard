using System.Text.Json;

namespace PlayBoard.Services
{
    public class JsonUserStore : IUserStore
    {
        private readonly string _dataFile;
        private static readonly SemaphoreSlim _fileLock = new(1, 1);

        public JsonUserStore()
        {
            _dataFile = Path.Combine(AppContext.BaseDirectory, "DataCollection", "UserData.json");
        }

        public async Task<string?> GetPasswordHashAsync(string normalizedUserName)
        {
            var users = await LoadUsersAsync();
            var result = users.Where(x=>x.Key.ToLower() == normalizedUserName.ToLower()).FirstOrDefault();
            return result.Value;
        }

        public async Task<bool> ExistsAsync(string normalizedUserName)
        {
            var users = await LoadUsersAsync();
            return users.ContainsKey(normalizedUserName);
        }

        public async Task AddAsync(string normalizedUserName, string passwordHash)
        {
            await _fileLock.WaitAsync();
            try
            {
                var users = await LoadUsersAsync();
                users[normalizedUserName] = passwordHash;
                await File.WriteAllTextAsync(_dataFile, JsonSerializer.Serialize(users));
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task<Dictionary<string, string>> LoadUsersAsync()
        {
            if (!File.Exists(_dataFile))
                return new Dictionary<string, string>();

            var json = await File.ReadAllTextAsync(_dataFile);
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
