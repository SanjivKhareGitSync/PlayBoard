using PlayBoard.ModelCollection;

namespace PlayBoard.Services
{
    public interface IAuthService
    {
        bool VerifyCredentials(LoginRequest loginRequest);
        RegistrationResult RegisterUser(RegistrationForm registrationForm);
    }

    public enum RegistrationResult
    {
        Success,
        UserAlreadyExists,
        InvalidInput,
        Error
    }
}
