using Mango.Web.Models;
using static Mango.Web.Utility.SD;

namespace Mango.Web.Service.IService
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService _baseService;
        public AuthService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
          return await _baseService.SendAsync(new RequestDto()
            {
              ApiType = ApiType.POST,
              Url = AuthAPIBase + "/api/auth/login",
              Data = loginRequestDto
            });
        }

        public async Task<ResponseDto> RegisterAsync(RegistrationRequestDto registrationRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Url = AuthAPIBase + "/api/auth/register",
                Data = registrationRequestDto
            });
        }

        public async Task<ResponseDto> AssignRoleAsync(RegistrationRequestDto registrationRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Url = AuthAPIBase + "/api/auth/AssignRole",
                Data = registrationRequestDto
            });
        }
    }
}
