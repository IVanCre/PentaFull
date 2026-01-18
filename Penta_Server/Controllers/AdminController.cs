using Penta_Server.Interfaces;
using Penta_Server.StaticUtilits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Penta_Server.Controllers
{
    [Route("Admin")]
    [ApiController]
    [Authorize(Roles = RoleNames.Admin)]
    public class AdminController(
        ILogReader logReader) : ControllerBase
    {
        private ILogReader _logReader = logReader;


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




        //get work statistic
    }
}
