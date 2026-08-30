namespace PlayBoard.Services
{
    public interface IUserStore
    {
        Task<string?> GetPasswordHashAsync(string normalizedUserName);
        Task<bool> ExistsAsync(string normalizedUserName);
        Task AddAsync(string normalizedUserName, string passwordHash);
    }
}