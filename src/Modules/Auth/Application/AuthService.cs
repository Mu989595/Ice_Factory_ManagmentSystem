using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IcePlant.Application.DTOs;
using IcePlant.Application.Interfaces;
using IcePlant.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IcePlant.Application.Services;

/// <summary>
/// Simple PIN-based authentication.
/// Validates against a system_pin.txt file created by the user on first launch.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly string _pinFilePath = Path.Combine(Directory.GetCurrentDirectory(), "system_pin.txt");

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<bool> IsSystemInitializedAsync()
    {
        return Task.FromResult(File.Exists(_pinFilePath));
    }

    public async Task<Result<AuthResponseDto>> SetupSystemAsync(string pin)
    {
        if (File.Exists(_pinFilePath))
            return Result.Failure<AuthResponseDto>("System is already initialized.");

        if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4)
            return Result.Failure<AuthResponseDto>("كلمة السر يجب أن تكون 4 رموز على الأقل.");

        await File.WriteAllTextAsync(_pinFilePath, pin);
        
        var result = GenerateToken();
        return Result.Success(result);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        if (!File.Exists(_pinFilePath))
            return Result.Failure<AuthResponseDto>("النظام غير مهيأ بعد. يرجى إعداد كلمة السر أولاً.");

        var savedPin = await File.ReadAllTextAsync(_pinFilePath);

        if (request.Password != savedPin.Trim())
            return Result.Failure<AuthResponseDto>("كلمة السر غلط.");

        var result = GenerateToken();
        return Result.Success(result);
    }

    private AuthResponseDto GenerateToken()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "Owner")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(
            double.Parse(_configuration["JwtSettings:ExpirationInMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponseDto(
            Token: tokenString,
            Expiration: expiry);
    }
}
