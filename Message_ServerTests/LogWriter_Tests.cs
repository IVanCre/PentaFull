
using Microsoft.Extensions.Configuration;
using Message_Server.Services.Loggers;

namespace Message_ServerTests
{
    public class LogWriter_Tests
    {
        private IConfiguration config;

        public LogWriter_Tests()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Logging:LogLevel:Default", "Information"},
                {"Logging:LogLevel:Microsoft.AspNetCore", "Warning"},
                {"Logging:AutoDeleteIntervalMinutes", "60"},
                {"Logging:MaxDayToHold", "14"},
                {"Logging:FolderName", "TestLogs_1"}//создастся в проекте автотестов
            };

            config= new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }


        [Test,Order(1)]
        public void CreateFolder()
        {
            var logger = new LogWriter(config);
            Assert.Multiple(()=> 
            {
                Assert.That(Directory.Exists(logger.LogFolder), Is.EqualTo(true));
             });
        }

        [Test,Order(2)]
        public void SaveToFile_TEST()
        {
            var logger = new LogWriter(config);
            logger.SaveError("row_1");
            logger.SaveError("row_2");

            var fileName = Path.Combine(logger.LogFolder, $"Log_{DateTime.Now.Date.ToString("dd_MM_yyyy")}.txt");

            Thread.Sleep(1000);

            Assert.Multiple(() =>
            {
                Assert.That(true, Is.EqualTo(File.Exists(fileName)));

                string[] rows = File.ReadAllLines(fileName);
                Assert.That(rows.Length, Is.EqualTo(2));
                Assert.That(rows[0].Contains("row_1"), Is.EqualTo(true));
                Assert.That(rows[1].Contains("row_2"), Is.EqualTo(true));
            });
        }


        [Test, Order(100)]
        public void ClearTestObjects()
        {
            var logger = new LogWriter(config);
            Directory.Delete(logger.LogFolder,true);
        }
    }
}