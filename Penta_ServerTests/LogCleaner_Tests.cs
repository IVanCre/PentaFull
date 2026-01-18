
using Microsoft.Extensions.Configuration;
using Penta_Server.Services.Loggers;

namespace Penta_ServerTests
{
    public class LogCleaner_Tests
    {
        private IConfiguration config;

        private void CreateFile(string folder, string fileName)
        {
            var filePath = Path.Combine(folder, fileName);
            File.WriteAllLines(filePath, new string[] { "row_1", "row_2" });
        }
        public LogCleaner_Tests()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Logging:LogLevel:Default", "Information"},
                {"Logging:LogLevel:Microsoft.AspNetCore", "Warning"},
                {"Logging:AutoDeleteIntervalMinutes", "1"},
                {"Logging:MaxDayToHold", "0"},
                {"Logging:FolderName", "TestLogs_3"}//создастся в проекте автотестов
            };

            config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Directory.CreateDirectory(new LogReader(config).LogFolder);//т.к. реадер не отвечает за это
        }

        [Test,Order(1)]
        public void CleanFiles()
        {
            var cleaner = new LogFolderCleaner(null, config);
            CreateFile(cleaner.LogFolder, "testFile_11.txt");
            CreateFile(cleaner.LogFolder, "testFile_12.txt");
            CreateFile(cleaner.LogFolder, "testFile_13.txt");

            cleaner.Start();
            Thread.Sleep(90_000);

            Assert.That(Directory.GetFiles(cleaner.LogFolder).Length, Is.EqualTo(0));
        }

        [Test, Order(100)]
        public void ClearTestObjects()
        {
            var logger = new LogFolderCleaner(null, config);
            Directory.Delete(logger.LogFolder, true);
        }
    }
}
