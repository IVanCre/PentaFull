
using Microsoft.AspNetCore.Mvc;
using Message_Server.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Message_Server.Controllers
{
    [Route("User")]
    [ApiController]
    public class UserController(
        IUserRepository repo,
        ITokenManager tokenMngr,
        ICopyUserDetector copyLoginDtctr,
        ILogWriter logger) : ControllerBase
    {
        private readonly IUserRepository _userRepository = repo;
        private readonly ITokenManager _tokenMngr = tokenMngr;
        private readonly ICopyUserDetector _copyLoginDtctr = copyLoginDtctr;
        private readonly ILogWriter _logger = logger;



        [HttpPost("Registration")]
        public async Task<string> Registration(string name, string pass)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 50 || name.Length<1)
                throw new ArgumentException("Invalid name len");

            if (string.IsNullOrEmpty(pass) || pass.Length > 50 || pass.Length<1)
                throw new ArgumentException("Invalid password len");

            var userID = await _userRepository.AddNewUserAsync(name, pass);
            if(userID!=-1)
                return _tokenMngr.CreateToken(userID,name, pass);
            else
            {
                _logger.SaveWarning($"Отказ в регистрации - такой юзер({name}_{pass}) уже есть");
                return string.Empty;
            }
        }

        [HttpGet("Login")]
        public async Task<string> Login(string name, string pass)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 50 || name.Length < 1)
                throw new ArgumentException("Invalid name len");

            if (string.IsNullOrEmpty(pass) || pass.Length > 50 || pass.Length < 1)
                throw new ArgumentException("Invalid password len");

            var userID = await _userRepository.FindUserAsync(name, pass);
            if (userID != -1)
            {
                if (!_copyLoginDtctr.IsClientAlreadyInSystem(userID))//не позволяем подключаться одновременно с 2 и более
                {
                    return _tokenMngr.GetToken(name, pass);
                }
                _logger.SaveWarning($"Зафиксирована попытка входа в один аккаунт(name={name}) с нескольких устройств.Отказ");
            }
            return string.Empty;
        }

        [HttpPost("Logout")]
        [Authorize]
        public void Logout()
        {
            throw new NotImplementedException();
        }

        [HttpPost("DeleteSelfAccount")]
        [Authorize]
        public void DelSelfAccount()
        {
            string token = Request.Headers["Authorization"];
            token=token.Replace("Bearer ", "");
            _userRepository.DeleteUserByTokenAsync(token);
        }
    }
}
