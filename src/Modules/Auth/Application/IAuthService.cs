using IcePlant.Application.DTOs;
using IcePlant.Domain.Common;

namespace IcePlant.Application.Interfaces;

public interface IAuthService
{
    Task<bool> IsSystemInitializedAsync();
    Task<Result<AuthResponseDto>> SetupSystemAsync(string pin);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request);
}
