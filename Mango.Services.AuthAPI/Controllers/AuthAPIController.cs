using Mango.Fro.AuthAPI.Models;
using Mango.Fro.AuthAPI.Service.IService;
using Mango.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Mango.Fro.AuthAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;

        private readonly ResponseDto _response;
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;
        public AuthAPIController(IAuthService authService, IMessageBus messageBus, IConfiguration configuration)
        {
            _authService = authService;
            _response = new();
            _messageBus = messageBus;
            _configuration = configuration;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {
            var errorMessage = await _authService.Register(model);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;
                return BadRequest(_response);
            }
           _messageBus.PublishMessage(model.Email, _configuration.GetValue<string>("TopicAndQueueNames:EmailRegisterUserQueue"));
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            LoginResponseDto loginResponse; 
            loginResponse = await _authService.Login(model);
   
            if (loginResponse.User is null)
            {

                _response.IsSuccess = false;
                _response.Message = "Username or Password is incorrect";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            return Ok(_response);
        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDto model)
        {
            var isAssignRoleSucess = await _authService.AssignRole(model.Email, model.Role.ToUpper());

            if (!isAssignRoleSucess)
            {
                _response.IsSuccess = false;
                _response.Message = "Error encountered";
                return BadRequest(_response);
            }
            return Ok(_response);
        }
    }
}

