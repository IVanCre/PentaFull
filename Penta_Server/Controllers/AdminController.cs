using Penta_Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MessageLib;
using Microsoft.AspNetCore.Authorization;
using Penta_Server.StaticUtilits;

namespace Penta_Server.Controllers
{
    [Route("Admin")]
    [ApiController]
    [Authorize(Roles = RoleNames.Admin)]
    public class AdminController(
        ILogReader logReader,
        IClientNotifier notifier,
        IUserRepository userRepo) : ControllerBase
    {
        private ILogReader _logReader = logReader;
        private IClientNotifier _notifier = notifier;
        private IUserRepository _userRepository=userRepo;


        [HttpGet("GetLogsFiles")]
        public async Task<IEnumerable<string>> GetLogFiles()
        {
            return _logReader.GetLogFileNames();
        }

        [HttpGet("GetLogFile")]
        public async Task<IEnumerable<string>> GetLogs(string filePath)
        {
            return await _logReader.GetLogsFromFileAsync(filePath);
        }

        [HttpPost("NotifyAll")]
        public void SendNotificationToAll([FromBody] string message)
        {
            _= Task.Factory.StartNew(async () =>
            {
                var allUsers = await _userRepository.GetAllUsers();
                foreach (var userID in allUsers)
                    _notifier.SendToUser(MessageFactory.CreateNotify(userID,message));
            });

        }




        //get work statistic
    }
}
