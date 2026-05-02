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
        IConnectionsRepository connRepo,
        IUserRepository userRepo) : Hub
    {
        private IConnectionsRepository _connRepo = connRepo;//чтобы отслеживать ассоциацию пользователя и его подключение
        private ILogWriter _logger = logger;
        private IMessageProcessor _messageProc = messProcessor;
        private IClientNotifier _clientNotifier = clientNotifier;
        private IUserRepository _userRepo = userRepo;

        public override async Task OnConnectedAsync()//подключение клиента
        {
            var userMaskedID = Context.User.Claims.FirstOrDefault(x => x.Type == "userID")?.Value;//Проверяем сущность, котору сами добавили в токене
            if (!string.IsNullOrEmpty(userMaskedID))
            {
                var userID = int.Parse(userMaskedID);
                _logger?.SaveInfo($"Пользователь {userID} подключился к хабу с ConnectionId: {Context.ConnectionId}");
                _connRepo.Add(userID, Context.ConnectionId);
                _userRepo.SetUserLastConnectDate(userID);

                await base.OnConnectedAsync();
                await _clientNotifier?.SendAllNonSended(userID, Context.ConnectionId);//сразу отдаем накопившиеся сообщения
            }
            else
                _logger?.SaveInfo($"Отказ в подключении юзеру к хабу");
        }

        public override async Task OnDisconnectedAsync(Exception? exep)//отключение клиента
        {
            var userID = Context.User.Claims.FirstOrDefault(x => x.Type == "userID")?.Value;
            _logger?.SaveInfo($"Пользователь {userID} отключился ");

            var parsed = int.Parse(userID);
            _connRepo.RemoveByUserID(parsed);
            _userRepo.SetUserLastConnectDate(parsed);

            await base.OnDisconnectedAsync(null);

            if(exep!=null)
                _logger?.SaveError($"Ошибка при отключении юзера от хаба: {exep.Message}");
        }




        public void AcknowledgeReceived(Guid messageID)//подтверждение получения серверного сообщения клиентом
        {
            try
            {
                _clientNotifier.MessageSended(messageID);
                _logger?.SaveInfo($"Клиент подтвердил получение сообщения id={messageID}");
            }
            catch (Exception ex)
            {
                _logger?.SaveInfo($"Ошибка подтверждения получения сообщения: {ex.Message}");
            }
        }


        //это точка получения сервером сообщения ОТ клиента
        public void SendToServer(Message msg)//клиент пишет на этот хаб
        {
            msg.GenerateNewID();//присваиваем серверный ID
            _messageProc.ProcessingMessage(msg);
        }
    }
}
