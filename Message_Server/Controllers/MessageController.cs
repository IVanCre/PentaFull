using Message_Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Message_Server.Controllers
{
    [Route("Message")]
    [ApiController]
    public class MessageController(
        IMessageRepository msgRepo,
        ITokenManager tokenMngr) : ControllerBase
    {
        private readonly IMessageRepository _msgRepo = msgRepo;
        private readonly ITokenManager _tokenMngr= tokenMngr;


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

            if (userID!=-1)
                return _msgRepo.HasNonSended(userID);
            else
                return false;
        }
    }
}
