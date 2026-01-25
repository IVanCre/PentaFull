using Microsoft.AspNetCore.SignalR;

using Penta_Server.Interfaces;
using MessageLib;


namespace Penta_Server.Services.SignalR
{
    /// <summary>
    /// Осуществляет отправку сообщений клиентам
    /// </summary>
    /// <param name="messageRepo"></param>
    /// <param name="groupRepo"></param>
    public class ClientNotifier(
        IMessageRepository messageRepo,
        IConnectionsRepository connRepo,
        IHubContext<MessageHub> hubContext,
        IMessageSaver messSaver,
        ILogWriter logger) : IClientNotifier
    {
        private IMessageRepository _messageRepo= messageRepo;
        private IConnectionsRepository _connRepo = connRepo;
        private IMessageSaver _messSaver=messSaver;
        private ILogWriter _logger = logger;
        private IHubContext<MessageHub> _hubContext=hubContext;


        public async Task SendAllNonSended(int userID, string connID)//отправляет клиенту все неотправленные ЕМУ сообщения
        {
            if (_hubContext != null)
            {
                var nonsended = await _messageRepo.GetNonSendedForUserAsync(userID);
                if (nonsended != null && nonsended.Count > 0)
                {
                    var client = _hubContext.Clients.Client(connID);
                    if (client != null)
                    {
                        foreach (var msg in nonsended)
                            await client.SendAsync("RecieveMessage", msg);
                    }
                }
            }
        }
        public async Task SendToUser(Message msg)
        {
            _messSaver.Save(msg);//сохраняем в БД(вдруг хаба нет или связь плохая)

            var connectionID = _connRepo.GetConnectionID(msg.ToID);
            if (!string.IsNullOrEmpty(connectionID))//что такой юзер все еще подключен
            {
                var client = _hubContext.Clients.Client(connectionID);
                if (client != null)
                {
                    _logger?.SaveSystemInfo($"Пересылаем клиенту {connectionID} сообщение");
                    await client.SendAsync("RecieveMessage", msg);
                }
            }
        }


        public void MessageSended(int messageID) => _messageRepo.MarkForDelete(messageID);

    }
}
