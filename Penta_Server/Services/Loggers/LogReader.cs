using Penta_Server.Interfaces;
using System.Reflection;

namespace Penta_Server.Services.Loggers
{
    public class LogReader:ILogReader
    {
        public string LogFolder { get; private set; }

        public LogReader(IConfiguration config)
        {
            LogFolder = Path.Combine(AppContext.BaseDirectory, config["Logging:FolderName"]);
        }

        public IEnumerable<string> GetLogFileNames()
        {
            List<string> rows = new();
            if (Directory.Exists(LogFolder))
            {
                var finded = Directory.GetFiles(LogFolder);
                foreach(string path in finded)
                    rows.Add(Path.GetFileName(path));
            }
            return rows;
        }

        public async Task<IEnumerable<string>> GetLogsFromFileAsync(string fileName)
        {
            List<string> rows = new();
            if (Directory.Exists(LogFolder))
            {
                var filePaths = Directory.GetFiles(LogFolder);
                foreach (string f in filePaths)
                {
                    if (Path.GetFileName(f) == fileName)
                    {
                        var readed =await File.ReadAllLinesAsync(f);
                        rows.AddRange(readed);
                        break;
                    }
                }
            }                
            return rows;
        }
    }
}
