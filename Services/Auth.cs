using Microsoft.AspNetCore.Identity;
using PlayBoard.ModelCollection;
using System.Text.Json;

namespace PlayBoard.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _dataFile;
        private readonly IPasswordHasher<object> _passwordHasher;
        private static readonly object _fileLock = new();

        public AuthService(IPasswordHasher<object> passwordHasher)
        {
            _passwordHasher = passwordHasher;
            _dataFile = Path.Combine(AppContext.BaseDirectory, "DataCollection", "UserData.json");
        }

        public bool VerifyCredentials(LoginRequest loginRequest)
        {
            if (loginRequest is null)
                throw new ArgumentNullException(nameof(loginRequest));

            var users = LoadUsers();
            var key = loginRequest.UserName.Trim().ToLowerInvariant();

            if (!users.TryGetValue(key, out var storedHash))
                return false;

            var result = _passwordHasher.VerifyHashedPassword(new object(), storedHash, loginRequest.Password);
            return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
        }

        public RegistrationResult RegisterUser(RegistrationForm registrationForm)
        {
            if (registrationForm is null
                || string.IsNullOrWhiteSpace(registrationForm.UserName)
                || string.IsNullOrWhiteSpace(registrationForm.Password))
                return RegistrationResult.InvalidInput;

            lock (_fileLock)
            {
                var users = LoadUsers();
                var key = registrationForm.UserName.Trim().ToLowerInvariant();

                if (users.ContainsKey(key))
                    return RegistrationResult.UserAlreadyExists;

                users[key] = _passwordHasher.HashPassword(new object(), registrationForm.Password);

                try
                {
                    File.WriteAllText(_dataFile, JsonSerializer.Serialize(users));
                    return RegistrationResult.Success;
                }
                catch (IOException)
                {
                    return RegistrationResult.Error;
                }
            }
        }

        private Dictionary<string, string> LoadUsers()
        {
            if (!File.Exists(_dataFile))
                return new Dictionary<string, string>();

            var json = File.ReadAllText(_dataFile);
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                       ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
