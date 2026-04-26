namespace Penta_Server.Interfaces
{
    /// <summary>
    /// Читает логи и их наличие
    /// </summary>
    public interface ILogReader
    {
        public Task<IEnumerable<string>> GetLogFileNames();
        public Task<IEnumerable<string>> GetLogsFromFileAsync(string fileName);
    }
}
