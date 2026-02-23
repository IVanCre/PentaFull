using Penta_Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Penta_Server.Controllers
{
    [Route("Message")]
    [ApiController]
    public class MessageController(
        IMessageRepository msgRepo,
        ITokenManager tokenMngr,
        IDeviceTokenRepository deviceTknRepo,
        ILogWriter logger) : ControllerBase
    {
        private IDeviceTokenRepository _deviceTknRepo = deviceTknRepo;
        private IMessageRepository _msgRepo = msgRepo;
        private ITokenManager _tokenMngr= tokenMngr;
        private ILogWriter _logger = logger;


        /// <summary>
        /// Проверка наличия неотправленных сообщений, адресованных юзеру Х(опрашивает клиент)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet("FindUnreaded")]
        [Authorize]
        public async Task<bool> FindUnreaded()
        {
            int userID=-1;
            string token = Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(token))
            {
                token = token.Replace("Bearer ", "");
                userID=_tokenMngr.FindUserByToken(token);
            }

            if (userID != -1)
            {
                _logger?.SaveSystemInfo($"Получен запрос на проверку новых сообщений для юзера={userID}");
                return await _msgRepo.HasNonSended(userID);
            }
            else
                return false;
        }


        [HttpPost("SetDeviceForPush")]
        [Authorize]
        public async Task<bool> SetDeviceForPush(string tokenDevice)
        {
            int userID = -1;
            string token = Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(token))
            {
                token = token.Replace("Bearer ", "");
                userID = _tokenMngr.FindUserByToken(token);
            }

            if (userID != -1)
            {
                _logger?.SaveSystemInfo($"Юзер id={userID} добавил новое устройство для оповещений");
                return await _deviceTknRepo.SaveDeviceToken(userID,tokenDevice);
            }
            else
                return false;
        }
    }
}
