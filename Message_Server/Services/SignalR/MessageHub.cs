using Microsoft.AspNetCore.SignalR;

using Message_Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using MessageLib;

namespace Message_Server.Services.SignalR
{
    public interface IMessageHub
    {
        Task SendToServer(Message msg);
        Task SendToClient(Message msg);
        IHubCallerClients GetClientsProvider();

    }

    /// <summary>
    /// Обработчик клиентских подключений и запросов
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="repo"></param>
    /// 
    [Authorize]//чтобы неавторизованные(без токена) не смогли подключаться
    public class MessageHub(
        ILogWriter logger,
        IMessageProcessor messProcessor,
        IClientNotifier clientNotifier,
        IHubObserver hubObserver) : Hub, IMessageHub
    {
        private ConnectionsRepository _connRepo = new();//чтобы отслеживать ассоциацию пользователя и его подключение
        private ILogWriter _logger = logger;
        private IMessageProcessor _messageProc = messProcessor;
        private IClientNotifier _clientNotifier = clientNotifier;
        private IHubObserver _hubObserver = hubObserver;//чтобы получить текущий экземпляр хаба(если он есть)

        public override async Task OnConnectedAsync()//подключение клиента
        {
            _hubObserver.ClientConnected(this);

            var userMaskedID = Context.User.Claims.FirstOrDefault(x => x.Type == "userID")?.Value;//Проверяем сущность, котору сами добавили в токене
            if (!string.IsNullOrEmpty(userMaskedID))
            {
                var userID_int = int.Parse(userMaskedID);
                _logger?.SaveSystemInfo($"Пользователь {userID_int} подключился с ConnectionId: {Context.ConnectionId}");
                _connRepo.Add(userID_int, Context.ConnectionId);

                await base.OnConnectedAsync();
                await _clientNotifier?.SendAllNonSended(userID_int, Context.ConnectionId);//сразу отдаем накопившиеся сообщения
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exep)//отключение клиента
        {
            var userID = Context.User.Claims.FirstOrDefault(x => x.Type == "userID")?.Value;
            _logger?.SaveSystemInfo($"Пользователь {userID} отключился ");

            _connRepo.RemoveByUserID(int.Parse(userID));

            await base.OnDisconnectedAsync(null);

            _hubObserver.ClientDisconnected();
        }

        public async Task AcknowledgeReceived(int messageID) => _clientNotifier.MessageSended(messageID);//подтверждение получения сообщения от клиента



        public async Task SendToServer(Message msg) => await _messageProc.ProcessingMessage(msg);//клиент пишет на хаб
        public async Task SendToClient(Message msg)//хаб сам пишет клиенту
        {
            var connectionID = _connRepo.GetConnectionID(msg.ToID);
            if (!string.IsNullOrEmpty(connectionID))
            {
                var client = Clients.Client(connectionID);
                if (client != null)
                    await client.SendAsync("RecieveMessage", msg);
            }
        }

        public IHubCallerClients GetClientsProvider()
        {
            return Clients;
        }
    }
}
