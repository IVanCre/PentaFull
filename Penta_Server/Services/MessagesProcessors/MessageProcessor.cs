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
        private IUsersConnectionObserver _connObserver;

        public MessageProcessor(
            IGroupChatRepository groupRepo,
            IClientNotifier clientNotifier,
            IUserRepository userRepo,
            IMessageSaver messSaver,
            ILogWriter logger,
            IUsersConnectionObserver connObserver)
        {
            _messageSaver = messSaver;
            _messageSaver.MessageSaved += ProcessSavedMessage;//т.е. сообщение сохраняется, а потом мы отправляем ответ-результат
            _groupRepo = groupRepo;
            _clientNotifier = clientNotifier;
            _userRepo = userRepo;
            _logger = logger;
            _connObserver = connObserver;
        }

        //в этом методе поступившие сообщения обрабатываются и ставятся в очередь на сохранение 
        public void ProcessingMessage(Message msg)
        {
            switch (msg.Type)
            {
                //системные-одиночный ответ
                case MessageType.CreateGroupRequest:            CreateGroupChat(msg); break;
                case MessageType.DeleteSelfAccountRequest:      DeleteAccount(msg); break;
                case MessageType.StartObservRecieverConnect:    StartSendUserConnectionState(msg);break;
                case MessageType.EndObservRecieverConnect:      EndSendUserConnectionState(msg); break;

                //системные-групповой ответ
                case MessageType.AddToGroupRequest:             AddToGroupChatRequest(msg); break;
                case MessageType.RemoveUserFromGroupRequest:    RemoveUserFromGroupChatRequest(msg); break;
                case MessageType.DeleteGroupRequest:            DeleteGroupChatRequest(msg); break;
                case MessageType.SystemNotify:                  NotifyAll(msg);break;

                //пользовательские
                case MessageType.Text:
                case MessageType.Picture:
                case MessageType.Voice:
                    {
                        if (msg.ToID == -1 && msg.ChatID != -1)
                            SendToGroup(msg);//сообщение для группового чата
                        else
                            _messageSaver.Save(msg, msg.ID);//сообщение для конкретного юзера
                    }; break;
            }
        }

        private async void CreateGroupChat(Message msg)
        {
            _logger?.SaveInfo("Получен запрос на создание чата");
            var groupID = await _groupRepo.CreatGroupAsync(msg.FromID,msg.GetDataLikeString());
            if(groupID!=-1)
                _messageSaver.Save(MessageFactory.CreateGroupChat_Response(groupID, msg), msg.ID);//сохраняем в БД(вдруг хаба нет или связь плохая)
        }
        private async void DeleteAccount(Message msg)
        {
            _logger?.SaveInfo($"Получен запрос на удаление аккаунта id={msg.FromID}");
            var result = await _userRepo.DeleteUserByIDAsync(msg.FromID);
            if (result)
                _messageSaver.Save(MessageFactory.CreateDeleteAccountResponce(msg),msg.ID);
        }


        //в данной группе методов подразумевается массовый ответ, как следствие -оптимизированный способ хранения исходного запроса
 #region Groups 
        private async void AddToGroupChatRequest(Message msg)
        {
            _logger?.SaveInfo("Запрос добавление юзера в группу");
            var findedChat = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if (findedChat != null && findedChat.AdminGroupID==msg.FromID)//запрос от админа
            {
                if (await _groupRepo.AddUserToGroupAsync(msg.ToID, findedChat.ID))//после этого сущность findedChat имеет еще старый список
                {
                    _messageSaver.Save(MessageFactory.CreateUserAddedToGroupChat_ServerResponse(msg.ToID, findedChat.ID,findedChat.Name), msg.ID);//для добавляемого юзера

                    var sharedMarker = msg.ID;
                    foreach (var recieverID in findedChat.UserIDsInGroup())
                        _messageSaver.Save(MessageFactory.CreateAddUserToGroupChat_Response(msg, recieverID),sharedMarker);
                }
            }
        }
        private async void RemoveUserFromGroupChatRequest(Message msg)
        {
            _logger?.SaveInfo($"Запрос удаления юзера id={msg.ToID} из группы id={msg.ChatID}");
            var findedGroup = await _groupRepo.GetGroupByIDAsync(msg.ChatID);

            if (findedGroup != null && (
                findedGroup.AdminGroupID == msg.FromID ||// запрос идет от админа группы
                msg.FromID == msg.ToID))//или юзер сам хочет уйти
            {
                if (await _groupRepo.RemoveUserFromGroupAsync(msg.ToID, findedGroup.ID))
                {
                    var userInGroup = findedGroup.UserIDsInGroup();
                    var sharedMarker = msg.ID;
                    foreach (var recieverID in userInGroup)
                        _messageSaver.Save(MessageFactory.CreateDeleteUserFromGroupChat_Response(msg, recieverID),sharedMarker, userInGroup.Length);
                }
            }
        }
        private async void DeleteGroupChatRequest(Message msg)
        {
            _logger?.SaveInfo($"Получен запрос на удаление чата id={msg.ChatID}");
            var findedGroup = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if (findedGroup != null && findedGroup.AdminGroupID== msg.FromID)//только админ могет удалять
            {
                var sharedMarker = msg.ID;
                foreach (var recieverID in findedGroup.UserIDsInGroup())
                    _messageSaver.Save(MessageFactory.CreateDeleteGroupChat_Response(msg, recieverID),sharedMarker);

                _ = _groupRepo.DeleteGroup(msg.FromID, msg.ChatID);
            }
        }
        private async void SendToGroup(Message msg)
        {
            var findedGroup = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if (findedGroup != null)
            {
                var userInGroup = findedGroup.UserIDsInGroup();
                if (userInGroup.Contains(msg.FromID))//только действующий участник может писать в группу
                {
                    var sharedMarker = msg.ID;
                    foreach (var recieverID in userInGroup)
                    {
                        if (recieverID != msg.FromID)
                            _messageSaver.Save(MessageFactory.CreateResponseForGroupMember(msg, recieverID),sharedMarker, userInGroup.Length - 1);
                    }
                }
            }
        }
        private async void NotifyAll(Message msg)
        {
            var allUsers = await _userRepo.GetAllUsersID();
            Guid? sharedMarker = null;
            foreach (var recieverID in allUsers)
            {
                var finalMsg = MessageFactory.CreateNotify(recieverID, msg.GetDataLikeString());
                if (sharedMarker == null)
                    sharedMarker = finalMsg.ID;

                _messageSaver.Save(
                    finalMsg,
                    sharedMarker.Value,
                    allUsers.Count);
            }
        }
        #endregion


        //в этом методе берется последнее сохраненное сообщение из очереди сохраненных
        private void ProcessSavedMessage(Message savedMsg)
        {
            _ = _clientNotifier.SendToUserWithPush(savedMsg);
        }


#region UserConnectionObserver
        private void StartSendUserConnectionState(Message msg)
        {
            _connObserver.AddToObserve(msg.ChatID, msg.FromID, msg.ToID);//тут уже сам сервис будет отслеживать и увеодмлять получателя
        }
        private void EndSendUserConnectionState(Message msg)
        {
            _connObserver.DeleteFromObserve(msg.ChatID,msg.FromID,msg.ToID);
        }
        #endregion
    }
}
