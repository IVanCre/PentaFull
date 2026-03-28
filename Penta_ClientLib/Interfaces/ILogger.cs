

namespace Penta_ClientLib.Interfaces
{
    public enum LogEventType
    {
        Info,
        Warning,
        Error
    }
    internal interface ILogger//не хочу разделять на чтение-запись
    {
        /// <summary>
        /// Включение\отключение сбора логов
        /// </summary>
        /// <param name="canUse"></param>
        void CanUseLogs(bool canUse);
        void SaveMessage(LogEventType type, string text);
        event SysLogRecieved SysLogRecieved;
    }
}
