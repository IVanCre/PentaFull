using Penta_Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MessageLib;
using Microsoft.AspNetCore.Authorization;
using Penta_Server.StaticUtilits;
using Penta_Server.Services.MessagesProcessors;
using System.ComponentModel;


namespace Penta_Server.Controllers
{
    [Route("Admin")]
    [ApiController]
#if RELEASE//задрало уже токены вбивать при отладке))
    [Authorize(Roles = RoleNames.Admin)]
#endif
    public class AdminController(
        ILogReader logReader,
        IMessageProcessor messProc,
        MessagesDBCleaner cleaner) : ControllerBase
    {
        private ILogReader _logReader = logReader;
        private IMessageProcessor _messProc = messProc;
        private MessagesDBCleaner _messCleaner = cleaner;



        [HttpGet("GetLogsFiles")]
        public async Task<IEnumerable<string>> GetLogFiles()
        {
            return await _logReader.GetLogFileNames();
        }

        [HttpGet("GetLogFile")]
        public async Task<IEnumerable<string>> GetLogs(string filePath)
        {
            return await _logReader.GetLogsFromFileAsync(filePath);
        }

        [HttpPost("NotifyAll")]
        public void SendNotificationToAll([FromBody] string text)
        {
            _messProc.ProcessingMessage(
                new Message(//просто заглушка
                    -1,
                    -1,
                    -1,
                    -1,
                    MessageType.SystemNotify,
                    MessageUtils.TextToBytes(text),
                    DateTimeOffset.UtcNow,
                    false));
        }


        [HttpGet("GetUserList")]
        public async Task<List<string>> GetAllUsers(IUserRepository userRepo)
        {
            return await userRepo.GetAllUsersNames();
        }



#region манипуляции с БД
        [HttpDelete("DeleteMessages")]
        public void DeleteAllMessages()
        {
            _messCleaner.ClearAll();
        }
        [HttpPost("ResetMessagesTable")]
        public void ResetMessageTableIDAutoincrement()
        {
            _messCleaner.ResetMessageTableIDAutoincrement();
        }
        [HttpGet("PercentOfUsedMessageID")]
        public async Task<double> GetPercent()
        {
            return await _messCleaner.CheckIdentityLimit();
        }
        #endregion




        //get work statistic
    }
}
