using IcePlant.Application;
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

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("User registered: {Username}", request.Username);
                return Ok(ApiResponse<AuthResponseDto>.Ok(result.Value!));
            }

            _logger.LogWarning("Registration failed for {Username}: {Error}", request.Username, result.Error);
            return BadRequest(ApiResponse<object>.BadRequest(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, ApiResponse<object>.ServerError("An error occurred during registration."));
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("User logged in: {Username}", request.Username);
                return Ok(ApiResponse<AuthResponseDto>.Ok(result.Value!));
            }

            _logger.LogWarning("Login failed for {Username}: {Error}", request.Username, result.Error);
            return Unauthorized(ApiResponse<object>.Unauthorized(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ApiResponse<object>.ServerError("An error occurred during login."));
        }
    }
}
