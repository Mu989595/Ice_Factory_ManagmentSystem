using IcePlant.Application.DTOs;
using IcePlant.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var isInitialized = await _authService.IsSystemInitializedAsync();
        return Ok(new AuthStatusDto(isInitialized));
    }

    [HttpPost("setup")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Setup([FromBody] SetupRequestDto request)
    {
        try
        {
            var result = await _authService.SetupSystemAsync(request.Password);
            if (result.IsSuccess)
            {
                _logger.LogInformation("System PIN set up successfully.");
                return Ok(result.Value);
            }

            _logger.LogWarning("Failed PIN setup attempt: {Error}", result.Error);
            return BadRequest(new { error = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during setup");
            return StatusCode(500, new { error = "An error occurred during setup." });
        }
    }

    /// <summary>
    /// Unlock the system with the system PIN.
    /// </summary>
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
                _logger.LogInformation("System unlocked successfully.");
                return Ok(result.Value);
            }

            _logger.LogWarning("Failed unlock attempt.");
            return Unauthorized(new { error = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "An error occurred during login." });
        }
    }
}
