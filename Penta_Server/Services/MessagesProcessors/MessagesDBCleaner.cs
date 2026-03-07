using Penta_Server.Interfaces;
using Penta_Server.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using SysTimer =System.Timers.Timer;

namespace Penta_Server.Services.MessagesProcessors
{
    /// <summary>
    /// Циклично чистит БД от доставленных(юзерам) сообщений
    /// </summary>
    public class MessagesDBCleaner
    {
        private SysTimer _cleaner;
        private int minutesTimeout;
        private ILogWriter _logger;
        private string connStr;

        public MessagesDBCleaner(
            ILogWriter logger,
            IConfiguration config)
        {
            minutesTimeout = Convert.ToInt32(config["WorkDB:AutoCleanPeriodMinutes"]);
            _cleaner = new SysTimer(60_000 * minutesTimeout);
            _cleaner.Elapsed += Timer_Elapsed;
            _cleaner.AutoReset = true; // повторять
            _cleaner.Enabled = true;

            _logger = logger;
            connStr = config["WorkDB:ConnString"];
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            using (DB db= new DB(connStr))
            {
                int deletetd= db.Database.ExecuteSqlRaw("DELETE FROM Messages WHERE IsSended=1");
                if (deletetd > 0)
                    _logger?.SaveForDEBUG("Помеченные сообщения удалены");
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
