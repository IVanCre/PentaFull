using Microsoft.AspNetCore.SignalR;

using Message_Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using MessageLib;

namespace Message_Server.Services.SignalR
{
    /// <summary>
    /// Обработчик клиентских подключений и запросов
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="repo"></param>
    /// 
    [Authorize]//чтобы неавторизованные(без токена) не смогли подключаться
    public class ChatHub(
        ILogWriter logger,
        IConnectionsRepository repo,
        IMessageRepository msgRepo,
        IMessageProcessor groupMessProc) : Hub
    {
        private ILogWriter _logger = logger;
        private IMessageRepository _msgRepo =msgRepo;
        private IConnectionsRepository _connRepo = repo;//т.к. хаб создается новый для каждого нового клиента(подключения храним в общем объекте)
        private IMessageProcessor _messageProc = groupMessProc;



        public override async Task OnConnectedAsync()//подключение клиента
        {
            var userID = Context.User.Claims.FirstOrDefault(x => x.Type == "userID")?.Value;//Проверяем сущность, котору сами добавили в токене
            if (!string.IsNullOrEmpty(userID))
            {
                var userID_int = int.Parse(userID);
                _logger.SaveSystemInfo($"Пользователь {userID_int} подключился с ConnectionId: {Context.ConnectionId}");
                _connRepo.Add(userID_int, Context.ConnectionId);

                await base.OnConnectedAsync();

                await SendAllNonSended(userID_int, Context.ConnectionId);
            }
        }
        private async Task SendAllNonSended(int userID, string connID)//отправляет клиенту все неотправленные ЕМУ сообщения
        {
            var nonsended= await _msgRepo.GetNonSendedForUserAsync(userID);
            if (nonsended!=null && nonsended.Count > 0)
            {
                var client = Clients.Client(connID);
                if (client != null)
                {
                    foreach (var msg in nonsended)
                        await client.SendAsync("RecieveMessage", msg);
                }
            }
        }

        
        public override async Task OnDisconnectedAsync(Exception? exep)//отключение клиента
        {
            var userID = Context.User.Claims.FirstOrDefault(x => x.Type == "userID")?.Value;
            _logger.SaveSystemInfo($"Пользователь {userID} отключился ");

            _connRepo.RemoveByUserID(int.Parse(userID));

            await base.OnDisconnectedAsync(null);
        }

        public async Task AcknowledgeReceived(int messageID)//подтверждение получения сообщения от клиента
        {
            _msgRepo.MarkForDelete(messageID);
            _logger.SaveSystemInfo($"Пакет id={messageID} доставлен");
        }



        public async Task SendMessage(Message msg)//клиент отправляет сообщение в ЭТОТ хаб
        {
            await _messageProc.ProcessingMessage(msg, Clients);
        }
    }
}
