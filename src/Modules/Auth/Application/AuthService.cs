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
/// Validates the system PIN from configuration and issues a JWT token.
/// No database users, no Identity — just a single system password.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var systemPin = _configuration["SystemPin"];

        if (string.IsNullOrEmpty(systemPin))
            return Task.FromResult(
                Result.Failure<AuthResponseDto>("System PIN is not configured."));

        if (request.Password != systemPin)
            return Task.FromResult(
                Result.Failure<AuthResponseDto>("كلمة السر غلط."));

        var result = GenerateToken();
        return Task.FromResult(Result.Success(result));
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
