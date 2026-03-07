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
                            SendToGroup(msg);//сообщение для группового чата
                        else
                            _messageSaver.Save(msg);//сообщение для конкретного юзера
                    }; break;
            }
        }

        private async void CreateGroupChat(Message msg)
        {
            _logger?.SaveForDEBUG("Получен запрос на создание чата");
            var groupID = await _groupRepo.CreatGroupAsync(msg.FromID,msg.GetDataLikeString());
            if(groupID!=-1)
                _messageSaver.Save(MessageFactory.CreateGroupChat_Response(groupID, msg));//сохраняем в БД(вдруг хаба нет или связь плохая)
        }
        private async void DeleteAccount(Message msg)
        {
            _logger?.SaveForDEBUG($"Получен запрос на удаление аккаунта id={msg.FromID}");
            var result = await _userRepo.DeleteUserByIDAsync(msg.FromID);
            if (result)
                _messageSaver.Save(MessageFactory.DeleteAccountResponce(msg));
        }


        //в данной группе методов подразумевается массовый ответ, как следствие -оптимизированный способ хранения исходного запроса
 #region Groups 
        private async void AddToGroupChatRequest(Message msg)
        {
            _logger?.SaveForDEBUG("Запрос добавление юзера в группу");
            var findedChat = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if (findedChat != null)//указанная группа есть
            {
                if (await _groupRepo.AddUserToGroupAsync(msg.ToID, findedChat.ID))//после этого сущность findedChat имеет еще старый список
                {
                    _messageSaver.Save(MessageFactory.UserAddedToGroupChat_ServerResponse(msg.ToID, findedChat.ID,findedChat.Name));//для юзера
                    foreach (var recieverID in findedChat.UserIDsInGroup())
                    {
                        if(recieverID!= msg.ToID)
                            _messageSaver.Save(MessageFactory.AddUserToGroupChat_Response(msg, recieverID));
                    }
                }
            }
        }
        private async void RemoveUserFromGroupChatRequest(Message msg)
        {
            _logger?.SaveForDEBUG("Запрос удаления юзера из группы");
            var findedGroup = await _groupRepo.GetGroupByIDAsync(msg.ChatID);

            if (findedGroup != null && (
                findedGroup.AdminGroupID == msg.FromID ||// запрос идет от админа группы
                msg.FromID == msg.ToID))//или юзер сам хочет уйти
            {
                if (await _groupRepo.RemoveUserFromGroupAsync(msg.ToID, findedGroup.ID))
                {
                    foreach (var recieverID in findedGroup.UserIDsInGroup())
                        _messageSaver.Save(MessageFactory.DeleteUserFromGroupChat_Response(msg, recieverID));
                }
            }
        }
        private async void DeleteGroupChatRequest(Message msg)
        {
            _logger?.SaveForDEBUG($"Получен запрос на удаление чата id={msg.ChatID}");
            var findedGroup = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if (findedGroup != null)
            {
                if (await _groupRepo.DeleteGroup(msg.FromID, msg.ChatID))
                {
                    foreach (var recieverID in findedGroup.UserIDsInGroup())
                        _messageSaver.Save(MessageFactory.DeleteGroupChat_Response(msg, recieverID));
                }
            }
        }
        private async void SendToGroup(Message msg)
        {
            var findedGroup = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if (findedGroup != null)
            {
                foreach (var recieverID in findedGroup.UserIDsInGroup())
                {
                    if(recieverID!=msg.FromID)
                        _messageSaver.Save(MessageFactory.CreateResponseForGroupMember(msg, recieverID));
                }
            }
        }
        #endregion



        //в этом методе берется последнее сохраненное сообщение из очереди сохраненных
        private void ProcessSavedMessage(Message savedMsg)
        {
            _ = _clientNotifier.SendToUser(savedMsg);
        }
    }
}
