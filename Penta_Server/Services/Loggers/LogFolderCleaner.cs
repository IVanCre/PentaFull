using Penta_Server.Interfaces;
using System.Reflection;
using SysTimer = System.Timers.Timer;

namespace Penta_Server.Services.Loggers
{
    public class LogFolderCleaner
    {
        public string LogFolder { get; private set; }
        private SysTimer _cleaner;
        private int _minutesInterval = 60;
        private int _maxDaysSave = 14;
        private readonly ILogWriter _logWriter;

        public LogFolderCleaner(
            ILogWriter logger,
            IConfiguration config)
        {
            LogFolder = Path.Combine(AppContext.BaseDirectory, config["Logging:FolderName"]);

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
            if (Directory.Exists(LogFolder))
            {
                var files = Directory.GetFiles(LogFolder);
                var curDay = DateTime.Now;
                foreach (var file in files)
                {
                    if ((curDay.Date - new FileInfo(file).CreationTime.Date).TotalDays >= _maxDaysSave)
                    {
                        File.Delete(file);
                        _logWriter?.SaveInfo($"Logfile {file} auto deletetd");
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
