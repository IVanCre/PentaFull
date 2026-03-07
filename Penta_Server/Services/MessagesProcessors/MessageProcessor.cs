using Penta_Server.Interfaces;
using MessageLib;
using Penta_Server.Services.Repositories.Models;


namespace Penta_Server.Services.MessagesProcessors
{
    /// <summary>
    /// Осуществляет обработку сообщений(пользовательские и системные)
    /// </summary>
    /// <param name="groupRepo"></param>
    /// <param name="clientNotifier"></param>
    public class MessageProcessor : IMessageProcessor
    {
        private IMessageSaver _messageSaver;
        private IGroupChatRepository _groupRepo;
        private IClientNotifier _clientNotifier;
        private IUserRepository _userRepo;
        private ILogWriter _logger;

        public MessageProcessor(
            IGroupChatRepository groupRepo,
            IClientNotifier clientNotifier,
            IUserRepository userRepo,
            IMessageSaver messSaver,
            ILogWriter logger)
        {
            _messageSaver = messSaver;
            _messageSaver.MessageSaved += ProcessSavedMessage;//т.е. сообщение сохраняется, а потом мы отправляем ответ-результат
            _groupRepo = groupRepo;
            _clientNotifier = clientNotifier;
            _userRepo = userRepo;
            _logger = logger;
        }

        //в этом методе поступившие сообщения обрабатываются и ставятся в очередь на сохранение 
        public void ProcessingMessage(Message msg)
        {
            switch (msg.Type)
            {
                //системные-одиночный ответ
                case MessageType.CreateGroupRequest: CreateGroupChat(msg); break;
                case MessageType.DeleteSelfAccountRequest: DeleteAccount(msg); break;

                //системные-групповой ответ
                case MessageType.AddToGroupRequest: AddToGroupChatRequest(msg); break;
                case MessageType.RemoveUserFromGroupRequest: RemoveUserFromGroupChatRequest(msg); break;
                case MessageType.DeleteGroupRequest: DeleteGroupChatRequest(msg); break;

                //пользовательские
                case MessageType.Text:
                case MessageType.Picture:
                case MessageType.Voice:
                    {
                        if (msg.ToID == -1 && msg.ChatID != -1)
                            SendToGroupRequest(msg);//сообщение для группового чата
                        else
                            _messageSaver.Save(msg);//сообщение для конкретного юзера
                    }; break;
            }
        }

        private async void CreateGroupChat(Message msg)
        {
            _logger?.SaveSystemInfo("Получен запрос на создание чата");
            var groupID = await _groupRepo.CreatGroupAsync(msg.FromID,msg.GetDataLikeString());

            var createdMsg = MessageFactory.CreateGroupChat_Response(groupID, msg);
            _messageSaver.Save(createdMsg);//сохраняем в БД(вдруг хаба нет или связь плохая)
        }
        private async void DeleteAccount(Message msg)
        {
            _logger?.SaveSystemInfo($"Получен запрос на удаление аккаунта id={msg.FromID}");
            var result = await _userRepo.DeleteUserByIDAsync(msg.FromID);
            if (result)
            {
                var createdMsg = MessageFactory.DeleteAccountResponce(msg);
                _messageSaver.Save(createdMsg);
            }
        }


        //в данной группе методов подразумевается массовый ответ, как следствие -оптимизированный способ хранения исходного запроса
 #region Groups 
        private async void AddToGroupChatRequest(Message msg)
        {
            _logger?.SaveSystemInfo("Запрос добавление юзера в группу");
            var findedChat = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if (findedChat != null)//указанная группа есть
            {
                if (await _groupRepo.AddUserToGroupAsync(msg.ToID, findedChat.ID))//после этого сущность findedChat имеет еще старый список
                {
                    _messageSaver.Save(MessageFactory.UserAddedToGroupChat_ServerResponse(msg.ToID, findedChat.ID,findedChat.Name));//для юзера
                    _messageSaver.SaveOptimizedCopy(msg, findedChat.UserIDsInGroup());
                }
            }
        }
        private async void RemoveUserFromGroupChatRequest(Message msg)
        {
            _logger?.SaveSystemInfo("Запрос удаления юзера из группы");
            var findedGroup = await _groupRepo.GetGroupByIDAsync(msg.ChatID);

            if (findedGroup != null && (
                findedGroup.AdminGroupID == msg.FromID ||// запрос идет от админа группы
                msg.FromID == msg.ToID))//или юзер сам хочет уйти
            {
                if (await _groupRepo.RemoveUserFromGroupAsync(msg.ToID, findedGroup.ID))
                    _messageSaver.SaveOptimizedCopy(msg,findedGroup.UserIDsInGroup());
            }
        }
        private async void DeleteGroupChatRequest(Message msg)
        {
            _logger?.SaveSystemInfo($"Получен запрос на удаление чата id={msg.ChatID}");
            var findedGroup = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if (findedGroup != null)
            {
                if (await _groupRepo.DeleteGroup(msg.FromID, msg.ChatID))
                    _messageSaver.SaveOptimizedCopy(msg, findedGroup.UserIDsInGroup());
            }
        }
        private async void SendToGroupRequest(Message msg)
        {
            var findedGroup = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if (findedGroup != null)
                _messageSaver.SaveOptimizedCopy(msg, findedGroup.UserIDsInGroup());
        }
#endregion






        //в этом методе берется последнее сохраненное сообщение из очереди сохраненных
        private async void ProcessSavedMessage(Message savedMsg)
        {
            if (savedMsg.ChatID!=-1 )//значит групповая отправка
            {
                var recieversID = await _messageSaver.GetRecieversID(savedMsg);

                _ = Parallel.ForEach(recieversID, recieverID =>
                {
                    Message responseMsg=null;
                    switch (savedMsg.Type)//генерируем ответ прямо тут на основе типа запроса
                    {
                        case MessageType.AddToGroupRequest: 
                            {responseMsg= MessageFactory.AddUserToGroupChat_Response(savedMsg, recieverID); } break;
                        
                        case MessageType.RemoveUserFromGroupRequest:    
                            {responseMsg= MessageFactory.DeleteUserFromGroupChat_Response(savedMsg, recieverID); } break;
                        
                        case MessageType.DeleteGroupRequest:            
                            {responseMsg= MessageFactory.DeleteGroupChat_Response(savedMsg, recieverID); } break;

                        case MessageType.Text:
                        case MessageType.Picture:
                        case MessageType.Voice:
                            {responseMsg = MessageFactory.CreateResponseForGroupMember(savedMsg, recieverID);}break;
                    }

                    _ = _clientNotifier.SendToUser(responseMsg);//отправляем другое сообщение, но с исходным message.ID (чтобы потом понять. какое сообщение дошло)
                });
            }
            else
                _ = _clientNotifier.SendToUser(savedMsg);
        }
    }
}
