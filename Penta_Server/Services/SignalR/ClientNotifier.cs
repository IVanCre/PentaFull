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
        ILogWriter logger,
        IPushManager pushMngr) : IClientNotifier
    {
        private IPushManager _pushMngr = pushMngr;
        private IMessageRepository _messageRepo= messageRepo;
        private IConnectionsRepository _connRepo = connRepo;
        private ILogWriter _logger = logger;
        private IHubContext<MessageHub> _hubContext=hubContext;
        Dictionary<Guid, List<Guid>> _sendedPacks = new();

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
                        var idList = nonsended.Select(x => x.ID).ToList();//запоминаем, какие сообщения в пакете
                        _sendedPacks.Add(nonsended[0].ID, idList);
                        await client.SendAsync("RecieveMessagePack", nonsended);
                    }
                }
            }
        }
        public async Task SendToUserWithPush(Message msg)
        {
            var connectionID = _connRepo.GetConnectionID(msg.ToID);
            if (!string.IsNullOrEmpty(connectionID))//что такой юзер все еще подключен
            {
                var client = _hubContext.Clients.Client(connectionID);
                if (client != null)
                {
                    _logger?.SaveInfo($"Пересылаем клиенту {connectionID} сообщение");
                    await client.SendAsync("RecieveMessage", msg);
                }
            }
            else//пытаемся отослать пуш-уведомление(чтобы юзер открыл приложение и получил сообщение)
            {
                _ = _pushMngr.SendPushToUserDevices(msg.ToID,msg.FromID);
            }
        }
        public async Task SendToUserWithoutPush(Message msg)
        {
            var connectionID = _connRepo.GetConnectionID(msg.ToID);
            if (!string.IsNullOrEmpty(connectionID))//что такой юзер все еще подключен
            {
                var client = _hubContext.Clients.Client(connectionID);
                if (client != null)
                {
                    _logger?.SaveInfo($"Пересылаем клиенту {connectionID} сообщение");
                    await client.SendAsync("RecieveMessage", msg);
                }
            }
        }



        public void MessageSended(Guid messageID) => _messageRepo.DeleteMessage(messageID);
        public void PackSended(Guid messagePackID)
        {
            if(_sendedPacks.ContainsKey(messagePackID))
            {
                Task.Run(() =>
                {
                    foreach (var id in _sendedPacks[messagePackID])
                        _messageRepo.DeleteMessage(id);
                });
            }
        }
    }
}
