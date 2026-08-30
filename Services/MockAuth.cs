using Microsoft.AspNetCore.Identity;
using PlayBoard.ModelCollection;
using System.Text.Json;

namespace PlayBoard.Services
{
    public class MockAuthService : IAuthService
    {
        private readonly IUserStore _userStore;
        private readonly IPasswordHasher<object> _passwordHasher;

        public MockAuthService(IUserStore userStore, IPasswordHasher<object> passwordHasher)
        {
            _userStore = userStore;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> VerifyCredentialsAsync(LoginRequest loginRequest)
        {
            return true;
        }

        public async Task<RegistrationResult> RegisterUserAsync(RegistrationForm registrationForm)
        {
            if (registrationForm is null
                || string.IsNullOrWhiteSpace(registrationForm.UserName)
                || string.IsNullOrWhiteSpace(registrationForm.Password))
                return RegistrationResult.InvalidInput;

            var key = registrationForm.UserName.Trim().ToLowerInvariant();

            if (await _userStore.ExistsAsync(key))
                return RegistrationResult.UserAlreadyExists;

            var hash = _passwordHasher.HashPassword(new object(), registrationForm.Password);

            try
            {
                await _userStore.AddAsync(key, hash);
                return RegistrationResult.Success;
            }
            catch (IOException)
            {
                return RegistrationResult.Error;
            }
        }
    }
}
