using Mango.Fro.AuthAPI.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace Mango.Fro.AuthAPI.Service.IService
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<string> Register(RegistrationRequestDto registrationRequestDto);
        Task<bool> AssignRole(string email, string role);

    }
}
