namespace Penta_Server.Interfaces
{
    /// <summary>
    /// Пишет события в файлы
    /// </summary>
    public interface ILogWriter
    {
        public void SaveError(string text);
        public void SaveWarning(string text);
        public void SaveInfo(string text);
    }
}
