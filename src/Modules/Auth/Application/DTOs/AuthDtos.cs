namespace IcePlant.Application.DTOs;

/// <summary>
/// Request to setup the initial PIN
/// </summary>
public record SetupRequestDto(string Password);

/// <summary>
/// Status of the auth system
/// </summary>
public record AuthStatusDto(bool IsInitialized);

/// <summary>
/// Simple PIN-based login — no username needed.
/// </summary>
public record LoginRequestDto(
    string Password);

/// <summary>
/// Response after successful login or setup.
/// </summary>
public record AuthResponseDto(
    string Token,
    DateTime Expiration);
