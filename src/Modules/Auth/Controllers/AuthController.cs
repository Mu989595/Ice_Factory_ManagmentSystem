using IcePlant.Application.DTOs;
using IcePlant.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IWebHostEnvironment env,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Register is only available in Development (first admin setup).
    /// Disabled in Production — returns 404.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!_env.IsDevelopment())
        {
            _logger.LogWarning("Registration attempt blocked — not allowed outside Development.");
            return NotFound();
        }

        try
        {
            var result = await _authService.RegisterAsync(request);
            if (result.IsSuccess)
            {  return Ok(result.Value);
            }

            _logger.LogWarning("Registration failed for {Username}: {Error}", request.Username, result.Error);
            return BadRequest(new { error = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { error = "An error occurred during registration." });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("User logged in: {Username}", request.Username);
                return Ok(result.Value);
            }

            _logger.LogWarning("Login failed for {Username}: {Error}", request.Username, result.Error);
            return Unauthorized(new { error = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "An error occurred during login." });
        }
    }
}

                _logger.LogInformation("User registered: {Username}", request.Username);
              