using System.Security.Claims;
using VoltGuard.Application.DTOs.Auth;

namespace VoltGuard.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<CurrentUserDto?> GetCurrentUserAsync(ClaimsPrincipal userPrincipal);
}
