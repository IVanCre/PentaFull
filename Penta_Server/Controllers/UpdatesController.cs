
using Microsoft.AspNetCore.Mvc;
using Penta_Server.Interfaces;


namespace Penta_Server.Controllers
{
    [Route("Updates")]
    [ApiController]
    public class UpdatesController(
        IClientFileObserver fileObserver,
        ILogWriter logger) : ControllerBase
    {
        private IClientFileObserver _fileObserver = fileObserver;
        private ILogWriter _logger = logger;


        [HttpGet("GetNewestClientFileName")]
        public async Task<string> GetNewestClientFileName(string currentClientVersion, ClientType type)
        {
            try
            {
                _logger.SaveInfo($"Запрос новой версии клиента относительно версии: {currentClientVersion}");
                return await _fileObserver.GetNewClientVersionFileNameAsync(currentClientVersion, type);
            }
            catch (Exception ex)
            {
                _logger.SaveError($"Ошибка обработки запроса новой версии: {ex.Message}");
            }
            return string.Empty;
        }

        [HttpGet("LoadFile")]
        public async Task<IActionResult> DownloadFile(string fileName, ClientType type)
        {
            try
            {
                var stream = await _fileObserver.ReadFileAsync(fileName, type);
                if (stream != null)
                {
                    string contentType = string.Empty;
                    switch (type)
                    {
                        case ClientType.Android: contentType = "application/vnd.android.package-archive"; break;
                        case ClientType.Windows: contentType = "application/octet-stream"; break;
                    }
                    return File(stream, contentType, fileName);
                }
                else
                    return NotFound();
            }
            catch (Exception ex) 
            {
                _logger.SaveError($"Ошибка при попытке отдать файл клиенту: {ex.Message}");
                return NotFound();
            }
 
        }
    }
}
