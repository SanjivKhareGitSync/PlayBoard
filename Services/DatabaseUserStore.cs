using System.Text.Json;

namespace PlayBoard.Services
{
    public class DatabaseUserStore : IUserStore
    {
        public DatabaseUserStore()
        {
        }

        public async Task<string?> GetPasswordHashAsync(string normalizedUserName)
        {
            return "Hash Password";
        }

        public async Task<bool> ExistsAsync(string normalizedUserName)
        {
            return true;
        }

        public async Task AddAsync(string normalizedUserName, string passwordHash)
        {
            
        }

    }
}
