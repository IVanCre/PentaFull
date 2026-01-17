using Microsoft.AspNetCore.SignalR;

using Message_Server.Interfaces;
using MessageLib;
using Message_Server.Services.Repositories.Models;

namespace Message_Server.Services.SignalR
{
    /// <summary>
    /// Осуществляет отправку сообщений клиентам
    /// </summary>
    /// <param name="messageRepo"></param>
    /// <param name="groupRepo"></param>
    public class ClientNotifier(
        IMessageRepository messageRepo,
        IGroupRepository groupRepo,
        IHubObserver hubObserver,
        IMessageSaver messSaver) : IClientNotifier
    {
        private IMessageRepository _messageRepo= messageRepo;
        private IGroupRepository _groupRepo= groupRepo;
        private IHubObserver _hubObserver=hubObserver;
        private IMessageSaver _messSaver=messSaver;


        public async Task SendAllNonSended(int userID, string connID)//отправляет клиенту все неотправленные ЕМУ сообщения
        {
            var findedHub = _hubObserver.TryGetHub();
            if (findedHub != null)
            {
                var nonsended = await _messageRepo.GetNonSendedForUserAsync(userID);
                if (nonsended != null && nonsended.Count > 0)
                {
                    var client = findedHub.GetClientsProvider().Client(connID);
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
            _messSaver.Save(msg);//сохраняем в БД(вдруг хаба нет илои связь плохая)

            var messageHub = _hubObserver.TryGetHub();
            messageHub?.SendToClient(msg);
        }
        public async Task SendToGroup(Message msg)
        {
            var finded = await _groupRepo.GetGroupByIDAsync(msg.GroupID);
            if (finded != null)
            {
                var usersID = finded.UserIDsInGroup();
                foreach (var userID in usersID)
                {
                    await SendToUser(//для каждого юзера делаем отдельную копию сообщения
                        new Message(
                            -1,
                            msg.FromID,
                            msg.GroupID,
                            userID,
                            msg.Type,
                            msg.Data));
                }
            }
        }

        public void MessageSended(int messageID) => _messageRepo.MarkForDelete(messageID);

    }
}
