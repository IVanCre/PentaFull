
using Message_Server.Interfaces;
using System.Collections.Concurrent;
using System.Reflection;

namespace Message_Server.Services.Loggers
{
    public class LogWriter:ILogWriter
    {
        public string LogFolder { get; private set; }
        private ConcurrentQueue<string> _logs = new ConcurrentQueue<string>();
        private bool _writerWork = false;

        public LogWriter(IConfiguration config)
        {
            LogFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), config["Logging:FolderName"]);
            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);
        }

        public void SaveError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {DateTime.Now} {text}");
            AddToSave($"ERROR: {DateTime.Now} {text}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public void SaveInfo(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"INFO : {DateTime.Now} {text}");
            AddToSave($"INFO : {DateTime.Now} {text}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public void SaveSystemInfo(string text)
        {
#if DEBUG
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"DEBUG: {DateTime.Now} {text}");
            AddToSave($"DEBUG: {DateTime.Now} {text}");
            Console.ForegroundColor = ConsoleColor.White;
#endif
        }
        public void SaveWarning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"WARN : {DateTime.Now} {text}");
            AddToSave($"WARN : {DateTime.Now} {text}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void AddToSave(string text)
        {
            _logs.Enqueue(text);
            if (_logs.Count > 0 && !_writerWork)
            {
                _writerWork = true;
                Task.Factory.StartNew(() =>
                {
                    while (_logs.Count > 0)
                    {
                        _logs.TryDequeue(out string msg);
                        File.AppendAllText(Path.Combine(LogFolder,$"Log_{DateTime.Now.Date.ToString("dd_MM_yyyy")}.txt"), msg+"\n");                
                    }
                    _writerWork = false;
                });
            }
        }
    }
}
