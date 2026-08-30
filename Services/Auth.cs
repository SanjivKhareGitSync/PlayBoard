using Microsoft.AspNetCore.Identity;
using PlayBoard.ModelCollection;
using System.Text.Json;

namespace PlayBoard.Services
{

    public class AuthService : IAuthService
    {
        private readonly IUserStore _userStore;
        private readonly IPasswordHasher<object> _passwordHasher;

        public AuthService(IUserStore userStore, IPasswordHasher<object> passwordHasher)
        {
            _userStore = userStore;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> VerifyCredentialsAsync(LoginRequest loginRequest)
        {
            if (loginRequest is null)
                throw new ArgumentNullException(nameof(loginRequest));

            var key = loginRequest.UserName.Trim().ToLowerInvariant();
            var storedHash = await _userStore.GetPasswordHashAsync(key);
            if (storedHash is null)
                return false;

            var result = _passwordHasher.VerifyHashedPassword(new object(), storedHash, loginRequest.Password);
            return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
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
