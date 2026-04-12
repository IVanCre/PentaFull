using Penta_Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MessageLib;
using Microsoft.AspNetCore.Authorization;
using Penta_Server.StaticUtilits;


namespace Penta_Server.Controllers
{
    [Route("Admin")]
    [ApiController]
    //[Authorize(Roles = RoleNames.Admin)]
    public class AdminController(
        ILogReader logReader,
        IMessageProcessor messProc) : ControllerBase
    {
        private ILogReader _logReader = logReader;
        private IMessageProcessor _messProc = messProc;



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
                    DateTimeOffset.UtcNow));
        }




        //get work statistic
    }
}
