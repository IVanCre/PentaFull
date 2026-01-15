namespace Message_Server.Interfaces
{
    public interface ILogWriter
    {
        public void SaveError(string text);
        public void SaveWarning(string text);
        public void SaveInfo(string text);
        public void SaveSystemInfo(string text);
    }
}
