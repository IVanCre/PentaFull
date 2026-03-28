
using Penta_ClientLib.Interfaces;



namespace Penta_ClientLib.Services
{
    /// <summary>
    /// Предназеначен для сбора внутренних ошибок, для отладки в рантайме
    /// </summary>
    internal class SysLogger : ILogger
    {
        private bool _canWork = false;
        public event SysLogRecieved SysLogRecieved;

        public SysLogger()
        {
#if DEBUG
            _canWork = true;
#endif
        }

        public void CanUseLogs(bool canUse)
        {
            _canWork = canUse;
        }

        public void SaveMessage(LogEventType type, string text)
        {
            if (_canWork)
                SysLogRecieved?.Invoke(type, text, DateTime.Now);
        }
    }
}
