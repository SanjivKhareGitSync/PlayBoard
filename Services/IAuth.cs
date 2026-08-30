using PlayBoard.ModelCollection;

namespace PlayBoard.Services
{
    public interface IAuthService
    {
        Task<bool> VerifyCredentialsAsync(LoginRequest loginRequest);
        Task<RegistrationResult> RegisterUserAsync(RegistrationForm registrationForm);
    }

    public enum RegistrationResult
    {
        Success,
        UserAlreadyExists,
        InvalidInput,
        Error
    }
}
