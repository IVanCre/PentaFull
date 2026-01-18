
using Microsoft.Extensions.Configuration;
using Penta_Server.Services.Loggers;


namespace Penta_ServerTests
{
    internal class LogReader_Tests
    {
        private IConfiguration config;

        private void CreateFile(string folder,string fileName)
        {
            var filePath= Path.Combine(folder, fileName);
            File.WriteAllLines(filePath, new string[] { "row_1", "row_2" });
        }

        public LogReader_Tests()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Logging:LogLevel:Default", "Information"},
                {"Logging:LogLevel:Microsoft.AspNetCore", "Warning"},
                {"Logging:AutoDeleteIntervalMinutes", "60"},
                {"Logging:MaxDayToHold", "14"},
                {"Logging:FolderName", "TestLogs_2"}//создастся в проекте автотестов
            };
            config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Directory.CreateDirectory(new LogReader(config).LogFolder);//т.к. реадер не отвечает за это
        }

        [Test,Order(1)]
        public void ReadFileNames()
        {
            var logger = new LogReader(config);
            CreateFile(logger.LogFolder, "testLog_1.txt");
            CreateFile(logger.LogFolder, "testLog_2.txt");

            Assert.Multiple(() =>
            {
                Assert.That( Directory.Exists(logger.LogFolder), Is.EqualTo(true));
                Assert.That(logger.GetLogFileNames().Count(), Is.EqualTo(2));
            }); 
        }

        [Test,Order(2)]
        public void ReadFileRows()
        {
            var logger = new LogReader(config);
            CreateFile(logger.LogFolder, "testLog_3.txt");
            var rows = logger.GetLogsFromFileAsync("testLog_3.txt").Result;

            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(logger.LogFolder), Is.EqualTo(true));
                Assert.That(rows.Count(), Is.EqualTo(2));
            });
        }

        [Test, Order(100)]
        public void ClearTestObjects()
        {
            var logger = new LogReader(config);
            Directory.Delete(logger.LogFolder, true);
        }
    }
}
