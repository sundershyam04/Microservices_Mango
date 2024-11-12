using Mango.Services.Web.Service.IService;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly ResponseDto _responseDto;  
        private readonly IAuthService _authService;  
        private readonly ITokenProvider _tokenProvider;  
        public AuthController(IAuthService authService, ITokenProvider tokenProvider) 
        { 
            _authService = authService;
            _tokenProvider = tokenProvider;
            _responseDto = new ();
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto obj)
        {
            ResponseDto result = await _authService.LoginAsync(obj);
            if (result.Result !=null && result.IsSuccess)
            {
                LoginResponseDto? loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(result.Result));
                _tokenProvider.SetToken(loginResponseDto.Token);
                await SignInUser(loginResponseDto.Token);
                TempData["success"] = "Login successful";
                return  RedirectToAction("Index","Home");
            }
            else
            {
                TempData["error"] = result.Message;
                return View(obj);
            }
           
         }
        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            { new() {Text=SD.RoleAdmin, Value=SD.RoleAdmin},
              new() {Text=SD.RoleCustomer, Value=SD.RoleCustomer}
            };
            ViewBag.RoleList = roleList;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto obj)
        {
           ResponseDto result = await _authService.RegisterAsync(obj);
           ResponseDto assignRole;

            if (result != null && result.IsSuccess)
            {
                if (string.IsNullOrEmpty(obj.Role))
                {
                    obj.Role = SD.RoleCustomer;
                }
                assignRole = await _authService.AssignRoleAsync(obj);
                if (assignRole != null && assignRole.IsSuccess)
                {
                    TempData["success"] = "Registration success";
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                TempData["error"] = result.Message;
            }

            var roleList = new List<SelectListItem>()
            { new() {Text=SD.RoleAdmin, Value=SD.RoleAdmin},
              new() {Text=SD.RoleCustomer, Value=SD.RoleCustomer}
            };
            ViewBag.RoleList = roleList;
            return View(obj);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();    
            return RedirectToAction("Index","Home");
        }

        /// <summary>
        /// In order to set a User.Identity to be a logged in user(back-end). Sign-in using default .NET Identity : ASPNET core auth signs in respective user;(user details)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task SignInUser(string token)
        {
            // extract user data from token : read from security token 

            JwtSecurityTokenHandler handler = new();

            var jwt = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));
            
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Name).Value));

            identity.AddClaim(new Claim(ClaimTypes.Name,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));
            
            identity.AddClaim(new Claim(ClaimTypes.Role,
            jwt.Claims.FirstOrDefault(u => u.Type =="role" ).Value));


            // foreach (var c in jwt.Claims){
            //    Console.WriteLine($"ClaimType:{c.Type}       ClaimValue:{c.Value}");
            //}
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
