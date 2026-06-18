namespace IcePlant.Application.DTOs;

/// <summary>
/// Simple PIN-based login — no username needed.
/// </summary>
public record LoginRequestDto(
    string Password);

/// <summary>
/// Response after successful login.
/// </summary>
public record AuthResponseDto(
    string Token,
    DateTime Expiration);
