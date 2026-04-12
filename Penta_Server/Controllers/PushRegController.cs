using Penta_Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Penta_Server.Controllers
{
    [Route("PushRegistrator")]
    [ApiController]
    public class PushRegController(
        ITokenManager tokenMngr,
        IDeviceTokenRepository deviceTknRepo,
        ILogWriter logger) : ControllerBase
    {
        private IDeviceTokenRepository _deviceTknRepo = deviceTknRepo;
        private ITokenManager _tokenMngr= tokenMngr;
        private ILogWriter _logger = logger;


        [HttpPost("SetDevice")]
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
                _logger?.SaveInfo($"Юзер id={userID} добавил новое устройство для оповещений");
                return await _deviceTknRepo.SaveDeviceToken(userID,tokenDevice);
            }
            else
                return false;
        }
    }
}
