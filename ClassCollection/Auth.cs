using Microsoft.AspNetCore.Authorization;
using PlayBoard;
using PlayBoard.ModelCollection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PlayBoard.ClassCollection
{
    public class Auth
    {
        public Auth()
        {

        }

        public bool VerifyUserData(LoginRequest loginRequest)
        {
            if (loginRequest is null)
                throw new ArgumentNullException(nameof(loginRequest));
            var dataFile = Path.Combine(AppContext.BaseDirectory, "DataCollection", "UserData.json");

            if (!File.Exists(dataFile))
                return false;

            var json = File.ReadAllText(dataFile);
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                var users = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (users == null || users.Count == 0)
                    return false;

                foreach (var kvp in users)
                {
                    if (string.Equals(kvp.Key, loginRequest.UserName, StringComparison.OrdinalIgnoreCase)
                        && kvp.Value == loginRequest.Password)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public string RegisterUser(RegistrationForm registrationForm)
        {
            if (registrationForm is null || registrationForm.UserName is null || registrationForm.Password is null)
                return "user name/ password is null.";
            var dataFile = Path.Combine(AppContext.BaseDirectory, "DataCollection", "UserData.json");

            if (!File.Exists(dataFile))
            {
                return "user data file exist";
            }
            var json = File.ReadAllText(dataFile);
            if (string.IsNullOrWhiteSpace(json))
            {
                var users = new Dictionary<string, string>();
                users.Add(registrationForm.UserName, registrationForm.Password);
                var NewUserData = JsonSerializer.Serialize(users);
                File.WriteAllText(dataFile, NewUserData);
                return "1";
            }

            try
            {
                var users = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (users == null || users.Count == 0)
                {
                    users.Add(registrationForm.UserName, registrationForm.Password);
                    var NewUserData = JsonSerializer.Serialize(users);
                    File.WriteAllText(dataFile, NewUserData);
                    return "1";
                }
                foreach (var kvp in users)
                {
                    if (string.Equals(kvp.Key, registrationForm.UserName, StringComparison.Ordinal) && kvp.Value == registrationForm.Password)
                        return "User already exist";
                    users.Add(registrationForm.UserName, registrationForm.Password);
                    var NewUserData = JsonSerializer.Serialize(users);
                    File.WriteAllText(dataFile, NewUserData);
                    return "1";
                }

                return "Something went wrong";
            }
            catch (JsonException)
            {
                return "error serializing Json data";
            }
        }
    }
    public class FlagRequirement : IAuthorizationRequirement
    {
        // marker requirement — could carry parameters if needed
    }
    public class FlagAuthorizationHandler : AuthorizationHandler<FlagRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FlagRequirement requirement)
        {
            // 1) Preferred: check a claim issued at login
            if (context.User.HasClaim(c => c.Type == "Allowed" && c.Value == "true"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // 2) Alternatively: check another claim (e.g., role) or look up user by name and consult a data store.
            // Example fallback: allow if role Admin (optional)
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // otherwise do not call Succeed -> authorization fails
            return Task.CompletedTask;
        }
    }

}