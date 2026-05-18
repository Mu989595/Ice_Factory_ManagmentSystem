namespace IcePlant.Application.DTOs;

public record RegisterRequestDto(
    string Username,
    string Email,
    string Password,
    string FullName);

public record LoginRequestDto(
    string Username,
    string Password);

public record AuthResponseDto(
    string Token,
    DateTime Expiration,
    string Username,
    string FullName);
