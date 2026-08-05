using Microsoft.AspNetCore.Mvc;
using TaskListProject.Application;

namespace TaskReportApi.Controllers;

/// <summary>
/// API controller for user authentication
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly LoginHandler _loginHandler;
    private readonly ILogger<LoginController> _logger;

    public LoginController(LoginHandler loginHandler, ILogger<LoginController> logger)
    {
        _loginHandler = loginHandler;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="loginRequest">Login credentials</param>
    /// <returns>JWT token if authentication successful</returns>
    [HttpPost]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(loginRequest.User) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                _logger.LogWarning("Login attempt with empty credentials");
                return Unauthorized("Username and password are required");
            }

            var token = _loginHandler.Authenticate(loginRequest.User, loginRequest.Password);

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Invalid username or password");
            }

            return Ok(new LoginResponse
            {
                Token = token,
                User = loginRequest.User
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {User}", loginRequest.User);
            return StatusCode(500, "An error occurred during login");
        }
    }
}

/// <summary>
/// Login request model
/// </summary>
public class LoginRequest
{
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Login response model
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
}
