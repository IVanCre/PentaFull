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
        ILogWriter logger) : ControllerBase
    {
        private IMessageRepository _msgRepo = msgRepo;
        private ITokenManager _tokenMngr= tokenMngr;
        private ILogWriter _logger = logger;


        /// <summary>
        /// Проверка наличия неотправленных сообщений, адресованных юзеру Х
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
                _logger?.SaveSystemInfo("Получен запрос на проверку новых сообщений");
                return _msgRepo.HasNonSended(userID);
            }
            else
                return false;
        }
    }
}
