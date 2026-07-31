using Microsoft.Extensions.Logging;
using TaskListProject.Infrastructure.Data;

namespace TaskListProject.Application;

public class LoginHandler
{
    private readonly UserQueries _userQueries;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(UserQueries userQueries, ILogger<LoginHandler> logger)
    {
        _userQueries = userQueries;
        _logger = logger;
    }

    public string Authenticate(string user, string password)
    {
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Login attempt with empty credentials");
            return string.Empty;
        }

        _logger.LogInformation("Login attempt for user: {User}", user);

        var token = _userQueries.GetUserPassword(user, password);

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Failed login attempt for user: {User}", user);
            return string.Empty;
        }

        _logger.LogInformation("Successful login for user: {User}", user);
        return token;
    }

    public bool ValidateCredentials(string user, string password)
    {
        var token = Authenticate(user, password);
        return !string.IsNullOrEmpty(token);
    }
}
