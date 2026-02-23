using Microsoft.AspNetCore.SignalR;

using Penta_Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using MessageLib;

namespace Penta_Server.Services.SignalR
{
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
        IConnectionsRepository connRepo) : Hub
    {
        private IConnectionsRepository _connRepo = connRepo;//чтобы отслеживать ассоциацию пользователя и его подключение
        private ILogWriter _logger = logger;
        private IMessageProcessor _messageProc = messProcessor;
        private IClientNotifier _clientNotifier = clientNotifier;

        public override async Task OnConnectedAsync()//подключение клиента
        {
            var userMaskedID = Context.User.Claims.FirstOrDefault(x => x.Type == "userID")?.Value;//Проверяем сущность, котору сами добавили в токене
            if (!string.IsNullOrEmpty(userMaskedID))
            {
                var userID_int = int.Parse(userMaskedID);
                _logger?.SaveSystemInfo($"Пользователь {userID_int} подключился к хабу с ConnectionId: {Context.ConnectionId}");
                _connRepo.Add(userID_int, Context.ConnectionId);

                await base.OnConnectedAsync();
                await _clientNotifier?.SendAllNonSended(userID_int, Context.ConnectionId);//сразу отдаем накопившиеся сообщения
            }
            else
                _logger?.SaveSystemInfo($"Отказ в подключении юзеру к хабу");
        }

        public override async Task OnDisconnectedAsync(Exception? exep)//отключение клиента
        {
            var userID = Context.User.Claims.FirstOrDefault(x => x.Type == "userID")?.Value;
            _logger?.SaveSystemInfo($"Пользователь {userID} отключился ");

            _connRepo.RemoveByUserID(int.Parse(userID));

            await base.OnDisconnectedAsync(null);
        }

        public void AcknowledgeReceived(long messageID)
        {
            try
            {
                _clientNotifier.MessageSended(messageID);//подтверждение получения сообщения от клиента
                _logger?.SaveSystemInfo($"Клиент подтвердил получение сообщения id={messageID}");
            }
            catch (Exception ex)
            {
                _logger?.SaveSystemInfo($"Ошибка подтверждения получения сообщения: {ex.Message}");
            }
        }


        public void SendToServer(Message msg)
        {
            _messageProc.ProcessingMessage(msg);//клиент пишет на хаб
        }

        public IHubCallerClients GetClientsProvider()
        {
            return Clients;
        }
    }
}
