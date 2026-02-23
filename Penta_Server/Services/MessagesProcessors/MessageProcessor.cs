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
            _messageSaver.MessageSaved += ProcessSavedMessage;
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
                //user->server
                case MessageType.InviteToGroupResponce:      InviteToGroupChatResponce(msg); break;
                case MessageType.RemoveUserFromGroupRequest: RemoveUserFromGroupChatRequest(msg); break;
                case MessageType.CreateGroupRequest:         CreateGroupChat(msg); break;
                case MessageType.DeleteGroupRequest:         DeleteGroupChat(msg); break;
                case MessageType.DeleteSelfAccountRequest:   DeleteAccount(msg);break;

                //user->user
                case MessageType.InviteToGroupRequest: TryResendInvite(msg);break;

                case MessageType.Text:
                case MessageType.Picture:
                case MessageType.Voice: ResendToUsers(msg); break;
            }
        }

        private async void CreateGroupChat(Message msg)
        {
            _logger?.SaveSystemInfo("Получен запрос на создание чата");
            var groupID = await _groupRepo.CreatGroupAsync(msg.FromID,msg.GetDataLikeString());

            var createdMsg = MessageFactory.CreateGroupChat_Response(groupID, msg);
            _messageSaver.Save(createdMsg);//сохраняем в БД(вдруг хаба нет или связь плохая)
        }

        private async void InviteToGroupChatResponce(Message msg)
        {
            _logger?.SaveSystemInfo("Юзер ответил на приглашение в группу");
            if (msg.GetDataLikeBoolean())//юзер согласен вступить в группу
            {
                var finded = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
                if (finded != null)//указанная группа есть
                {
                    if (await _groupRepo.AddUserToGroupAsync(msg.FromID, finded.ID))
                    {
                        var usersID = finded.UserIDsInGroup();
                        foreach (var userID in usersID)//для каждого юзера делаем отдельную копию сообщения
                        {
                            var createdMsg = MessageFactory.InviteUserToGroupChat_ServerResponse(msg, userID);
                            _messageSaver.Save(createdMsg);
                        }
                    }
                }
            }
        }

        private async void RemoveUserFromGroupChatRequest(Message msg)
        {
            _logger?.SaveSystemInfo("Запрос удаления юзера из группы");
            var finded = await _groupRepo.GetGroupByIDAsync(msg.ChatID);

            if (finded != null && (
                finded.AdminGroupID == msg.FromID ||// запрос идет от админа группы
                msg.FromID == msg.ToID))//юзер сам хочет уйти
            {
                if (await _groupRepo.RemoveUserFromGroupAsync(msg.ToID, finded.ID))
                {
                    var usersID = finded.UserIDsInGroup();
                    foreach (var userID in usersID)//для каждого юзера делаем отдельную копию сообщения
                    {
                        var createdMsg = MessageFactory.DeleteUserFromGroupChat_Response(msg, userID);
                        _messageSaver.Save(createdMsg);
                    }
                }
            }
        }

        private async void DeleteGroupChat(Message msg)
        {
            _logger?.SaveSystemInfo($"Получен запрос на удаление чата id={msg.ChatID}");
            var finded = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if (finded != null)
            {
                var usersID = finded.UserIDsInGroup();
                if (await _groupRepo.DeleteGroup(msg.FromID, msg.ChatID))
                {
                    foreach (var userID in usersID)//для каждого юзера делаем отдельную копию сообщения
                    {
                        var createdMsg = MessageFactory.DeleteGroupChat_Response(msg, userID);
                        _messageSaver.Save(createdMsg);
                    }
                }
            }
        }

        private async void TryResendInvite(Message msg)
        {
            _logger?.SaveSystemInfo($"Юзер ответил на приглашение в чат id={msg.ChatID}");
            var group = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if(group!=null)
            {
                if(group.AdminGroupID== msg.FromID)//только создатель группы может приглашать в нее
                {
                    _messageSaver.Save(msg);
                }
            }
        }

        private async void ResendToUsers(Message msg)
        {
            if (msg.ToID == -1 && msg.ChatID != -1)//сообщение для группового чата
            {
                var group = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
                if (group != null)
                {
                    foreach (int userID in group.UserIDsInGroup())//если данные в сообщении большие, то для каждого юзера будет сохранена в БД копия сообщения(что не совсем эффективно)
                    {
                        var createdMsg = new Message(
                                Message.GenerateIDByTime(),
                                msg.FromID,
                                msg.ChatID,
                                userID,
                                msg.Type,
                                msg.Data,
                                msg.UtcTimestamp);
                        _messageSaver.Save(createdMsg);
                    }
                }
            }
            else//сообщение для конкретного юзера
            {
                _messageSaver.Save(msg);
            }
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


        //в этом методе берется последнее сохраненное сообщение
        private void ProcessSavedMessage(Message savedMsg)
        {
            _ = _clientNotifier.SendToUser(savedMsg);
        }
    }
}
