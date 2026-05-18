using IcePlant.Application.DTOs;
using IcePlant.Domain.Common;

namespace IcePlant.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request);
}
