using Microsoft.EntityFrameworkCore;
using Penta_Server.Interfaces;
using Penta_Server.Services.Repositories;
using SysTimer =System.Timers.Timer;

namespace Penta_Server.Services.MessagesProcessors
{
    /// <summary>
    /// Циклично чистит БД 
    /// </summary>
    public class MessagesDBCleaner
    {
        private SysTimer _cleaner;
        private int minutesTimeout;
        private ILogWriter _logger;
        private string connStr;
        private int _daysToHold;



        public MessagesDBCleaner(
            ILogWriter logger,
            IConfiguration config)
        {
            minutesTimeout = Convert.ToInt32(config["DBCleaner:AutoCleanPeriodMinutes"]);
            _daysToHold = Convert.ToInt32(config["DBCleaner:MessageLifePeriodDays"]);
            _cleaner = new SysTimer(60_000 * minutesTimeout);
            _cleaner.Elapsed += Timer_Elapsed;
            _cleaner.AutoReset = true; // повторять
            _cleaner.Enabled = true;

            _logger = logger;
            connStr = config["WorkDB:ConnString"];
        }

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            using (DB db= new DB(connStr))
            {
                var ids = await db.Messages
                    .Where(x => x.UtcTimestamp < DateTimeOffset.UtcNow.AddDays(-1*_daysToHold))
                    .Select(x => new { x.ID, x.SharedDataID })
                    .ToListAsync();

                var messageIds = ids.Select(x => x.ID).ToList();
                var sharedDataIds = ids.Select(x => x.SharedDataID).Distinct().ToList();

                // 2. Удаляем в правильном порядке
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    // Сначала сообщения (зависимая сущность)
                    int del_1= await db.Messages
                        .Where(m => messageIds.Contains(m.ID))
                        .ExecuteDeleteAsync();

                    // Затем общие данные (основная сущность)
                    int del_2 =await db.SharedDatas
                        .Where(s => sharedDataIds.Contains(s.ID))
                        .ExecuteDeleteAsync();

                    await transaction.CommitAsync();

                    if(del_1>0 || del_2>0)
                    _logger?.SaveInfo($"Вызвана очистка БД. Удалено сообщений={del_1}. Удалено данных={del_2}");
                }
                catch(Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.SaveError($"Ошибка при очистке БД от сообщений: {ex.Message}");
                }
            }
        }

        public async void ClearAll()
        {
            using (DB db = new DB(connStr))
            {
                await db.Messages
                    .ExecuteDeleteAsync();

                await db.SharedDatas
                    .ExecuteDeleteAsync();
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

        /// <summary>
        /// Сбрасывает счетчик автоинкремента ID в таблицах Mesage и SharedDatas, чтобы не упираться в int.max
        /// Делать ТОЛЬКО если таблицы пустые!
        /// </summary>
        public async void ResetMessageTableIDAutoincrement()
        {
            using (DB db = new DB(connStr))
            {
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    db.Database.ExecuteSqlRaw("TRUNCATE TABLE [Messages]");
                    db.Database.ExecuteSqlRaw("TRUNCATE TABLE [SharedDatas]");
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.SaveError(ex.Message);
                }
            }
        }

        public async Task<double> CheckIdentityLimit()
        {
            // Задаем порог, например, 80%
            double threshold = 80.0;

            long currentID = -1;
            using (DB db = new DB(connStr))
            {
                try
                {
                    currentID = await db.Database
                       .SqlQueryRaw<long>("SELECT CAST(IDENT_CURRENT('Messages') AS BIGINT) AS [Value]")
                       .FirstOrDefaultAsync();
                }
                catch(Exception ex)
                {
                    _logger.SaveError(ex.Message);
                }
            }

            double percentage = (double)currentID / long.MaxValue * 100;
            if (percentage > threshold)
                _logger.SaveWarning($"\nВнимание! Таблица Messages заполнена на {percentage:F2}% .Выполните очистку ключей!");

            return percentage;
        }
    }
}
