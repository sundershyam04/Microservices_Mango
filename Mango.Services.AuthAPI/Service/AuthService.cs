using Mango.Fro.AuthAPI.Data;
using Mango.Fro.AuthAPI.Models;
using Mango.Fro.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace Mango.Fro.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;

        }

        public async Task<bool> AssignRole(string email, string role)
        {
           var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == email);
            if (user != null)
            {
                if (!_roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                }
                //if user exists and role created
                await _userManager.AddToRoleAsync(user, role);
                return true;
            }
            return false;
         }
        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
           
                var user = _db.ApplicationUsers.FirstOrDefault<ApplicationUser>(u => u.UserName.ToLower() == loginRequestDto.UserName.ToLower());

                bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
          
            if (user is null || isValid is false)
            {
                return new LoginResponseDto() { User = null, Token = "" };
            }
            else
            {
                UserDto userDto = new()
                {
                    Email = user.Email,
                    Id = user.Id,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                };
                // Get roles of a current user
                var roles = await _userManager.GetRolesAsync(user);

                // Generate JWT if user exists with authentication success
                var token = _jwtTokenGenerator.GenerateToken(user,roles);

                LoginResponseDto loginResponseDto = new()
                {
                    User = userDto,
                    Token = token
                };
                return loginResponseDto;
            }
        }

        public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
        {
            ApplicationUser user = new()
            {
                Name = registrationRequestDto.Name,
                UserName = registrationRequestDto.Email,
                Email = registrationRequestDto.Email,
                NormalizedEmail = registrationRequestDto.Email.ToUpper(),
                PhoneNumber = registrationRequestDto.PhoneNumber
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);

                if (result.Succeeded)
                {
                    var userToReturn = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == registrationRequestDto.Email);

                    //UserDto userDto = new()
                    //{
                    //    Email = userToReturn.Email,
                    //    Id = userToReturn.Id,
                    //    Name = userToReturn.Name,
                    //    PhoneNumber = userToReturn.PhoneNumber
                    //};
                    // map appl user to user dto

                    return "";
                }

                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }
            }
            catch (Exception ex)
            {

            }
            return "Error encountered";
        }

    }

}

