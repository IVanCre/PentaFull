using Message_Server.Interfaces;
using System.Reflection;

namespace Message_Server.Services.Loggers
{
    internal class LogReader:ILogReader
    {
        private string _logFolder;

        public LogReader(IConfiguration config)
        {
            _logFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), config["Logging:FolderName"]);
        }

        public IEnumerable<string> GetLogFileNames()
        {
            List<string> rows = new();
            if (Directory.Exists(_logFolder))
            {
                var finded = Directory.GetFiles(_logFolder);
                foreach(string path in finded)
                    rows.Add(Path.GetFileName(path));
            }
            return rows;
        }

        public async Task<IEnumerable<string>> GetLogsFromFileAsync(string fileName)
        {
            List<string> rows = new();
            if (Directory.Exists(_logFolder))
            {
                var filePaths = Directory.GetFiles(_logFolder);
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
