
using Microsoft.AspNetCore.Mvc;
using Penta_Server.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Penta_Server.Controllers
{
    [Route("User")]
    [ApiController]
    public class UserController(
        IUserRepository repo,
        ITokenManager tokenMngr,
        ILogWriter logger) : ControllerBase
    {
        private readonly IUserRepository _userRepository = repo;
        private readonly ITokenManager _tokenMngr = tokenMngr;
        private readonly ILogWriter _logger = logger;


        [HttpPost("Registration")]
        public async Task<IActionResult> Registration(string name, string pass)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 50 || name.Length < 1)
                return BadRequest(new ArgumentException("Invalid name len"));

            if (string.IsNullOrEmpty(pass) || pass.Length > 50 || pass.Length < 1)
                return BadRequest(new ArgumentException("Invalid password len"));


            var userID = await _userRepository.AddNewUserAsync(name, pass);
            if (userID != -1)
            {
                _logger?.SaveForDEBUG($"Зарегистрирован новый юзер: {name}");
                var result =_tokenMngr.CreateTokenPack(userID, name, pass);
                return Ok(result); 
            }
            else
            {
                _logger?.SaveWarning($"Отказ в регистрации - такой юзер({name}_{pass}) уже есть");
                return Conflict(new ArgumentException());
            }
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(string name, string pass)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 50 || name.Length < 1)
                return BadRequest(new ArgumentException("Invalid name len"));

            if (string.IsNullOrEmpty(pass) || pass.Length > 50 || pass.Length < 1)
                return BadRequest(new ArgumentException("Invalid password len"));


            var userID = await _userRepository.FindUserAsync(name, pass);
            if (userID != -1)
            {
                _logger?.SaveForDEBUG($"Юзер успешно вошел в аккаунт в ручном режиме");
                var result = _tokenMngr.CreateTokenPack(userID, name, pass);
                return Ok(result);
            }
            else
            {
                _logger?.SaveWarning($"Отказ в регистрации - такой юзер({name}_{pass}) уже есть");
                return Conflict(new ArgumentException());
            }
        }


        [HttpPost("DeleteSelfAccount")]
        [Authorize]
        public async void DelSelfAccount()
        {
            string token = Request.Headers["Authorization"];
            token=token.Replace("Bearer ", "");
            var result =await _userRepository.DeleteUserByTokenAsync(token);
            if(result)
                _logger?.SaveForDEBUG("Юзер удалил свой аккаунт");
        }

        [HttpGet("RefreshToken")]
        public string[] RefreshToken()
        {
            string refreshToken = Request.Headers["Authorization"];
            var token = refreshToken.Replace("Bearer ", "");

            _logger?.SaveForDEBUG($"Поступил запрос на обновление токена");
            return _tokenMngr.RefreshJwtToken(token);
        }
    }
}
