using Message_Server.Interfaces;
using System.Reflection;
using SysTimer = System.Timers.Timer;

namespace Message_Server.Services.Loggers
{
    internal class LogFolderCleaner
    {
        private string _logFolder;
        private SysTimer _cleaner;
        private int _minutesInterval = 60;
        private int _maxDaysSave = 14;
        private readonly ILogWriter _logWriter;

        public LogFolderCleaner(
            ILogWriter logger,
            IConfiguration config)
        {
            _logFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), config["Logging:FolderName"]);

            _logWriter = logger;
            _minutesInterval = int.Parse(config["Logging:AutoDeleteIntervalMinutes"]);
            _maxDaysSave = int.Parse(config["Logging:MaxDayToHold"]);

            _cleaner = new SysTimer(60_000 * _minutesInterval);
            _cleaner.Elapsed += Timer_Elapsed;
            _cleaner.AutoReset = true;
            _cleaner.Enabled = true;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Directory.Exists(_logFolder))
            {
                var files = Directory.GetFiles(_logFolder);
                var curDay = DateTime.Now;
                foreach (var file in files)
                {
                    if ((curDay.Date - new FileInfo(file).CreationTime.Date).TotalDays > _maxDaysSave)
                    {
                        Directory.Delete(file);
                        _logWriter?.SaveSystemInfo($"Logfile {file} auto deletetd");
                    }
                }
            }
        }
        public void Start()
        {
            _cleaner.Start();
        }
        public void Stop()
        {
            _cleaner.Stop();
        }
    }
}
