namespace Message_Server.Interfaces
{
    public interface ILogReader
    {
        public IEnumerable<string> GetLogFileNames();
        public Task<IEnumerable<string>> GetLogsFromFileAsync(string fileName);
    }
}
