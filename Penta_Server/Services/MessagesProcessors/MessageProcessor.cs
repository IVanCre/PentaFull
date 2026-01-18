using Penta_Server.Interfaces;
using MessageLib;


namespace Penta_Server.Services.MessagesProcessors
{
    /// <summary>
    /// Осуществляет обработку сообщений(пользовательские и системные)
    /// </summary>
    /// <param name="groupRepo"></param>
    /// <param name="userRepo"></param>
    /// <param name="clientNotifier"></param>
    public class MessageProcessor(
        IGroupRepository groupRepo,
        IUserRepository userRepo,
        IClientNotifier clientNotifier) : IMessageProcessor
    {
        private IGroupRepository _groupRepo = groupRepo;
        private IUserRepository _userRepo = userRepo;
        private IClientNotifier _clientNotifier= clientNotifier;


        public async Task ProcessingMessage(Message msg)
        {
            if (msg.IsUserToUser())
                await _clientNotifier.SendToUser(msg);
            else 
                await ProcessMessageForGroup(msg);   
        }

        private async Task ProcessMessageForGroup(Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.EnterToGroupResponce:       EnterToGroupChatResponce(msg); break;
                case MessageType.LeaveGroupRequest:          LeaveGroupChatRequest(msg); break;
                case MessageType.RemoveUserFromGroupRequest: RemoveUserFromGroupChatRequest(msg); break;

                case MessageType.CreateGroupRequest:         CreateGroupChat(msg);break;
                case MessageType.DeleteGroupRequest:         DeleteGroupChat(msg);break;

                default: await _clientNotifier.SendToGroup(msg); break;
            }
        }


        private async void EnterToGroupChatResponce(Message msg)
        {
            if (msg.GetDataLikeBoolean())//юзер согласен вступить в группу
            {
                if (await _groupRepo.AddUserToGroupAsync(msg.FromID, msg.ChatID))
                {
                    var userName = await _userRepo.FindUserNameByIDAsync(msg.FromID);
                    _= _clientNotifier.SendToGroup(
                        new Message(//создаем новое сообщения для всех кто в группе
                            Message.GenerateIDByTime(),
                            msg.FromID,
                            msg.ChatID,
                            -1,
                            msg.Type,
                            MessageUtils.TextToBytes(userName)));//чтобы все знали, кто это вообще такой
                }
            }
        }
        private async void LeaveGroupChatRequest(Message msg)
        {
            if (await _groupRepo.GetGroupByIDAsync(msg.ChatID)!=null)
            {
                if (await _groupRepo.RemoveUserFromGroupAsync(msg.FromID, msg.ChatID))
                {
                    _ = _clientNotifier.SendToGroup(
                        new Message(//создаем новое сообщения для всех кто в группе
                            Message.GenerateIDByTime(),
                            msg.FromID,
                            msg.ChatID,
                            -1,
                            MessageType.LeaveGroupResponce,
                            MessageUtils.IntToBytes(msg.FromID)));//юзер которого удалился
                }
            }
        }
        private async void RemoveUserFromGroupChatRequest(Message msg)
        {
            var finded = await _groupRepo.GetGroupByIDAsync(msg.ChatID);
            if (finded != null && finded.AdminGroupID == msg.FromID)
            {
                if (await _groupRepo.RemoveUserFromGroupAsync(msg.ToID, msg.ChatID))
                {
                    _= _clientNotifier.SendToGroup(
                        new Message(//создаем новое сообщения для всех кто в группе
                            Message.GenerateIDByTime(),
                            msg.FromID,
                            msg.ChatID,
                            -1,
                            MessageType.RemoveUserFromGroupResponce,
                            MessageUtils.IntToBytes(msg.ToID)));//юзер которого удалили
                }
            }
        }


        private async void CreateGroupChat(Message msg)
        {
            var groupID = await _groupRepo.CreatGroupAsync(msg.FromID,msg.GetDataLikeString());

            _=_clientNotifier.SendToUser(
                new Message(//генерируем ответ для юзера, который прислал запрос
                    Message.GenerateIDByTime(),
                    -1,
                    groupID,//номер созданной группы
                    msg.FromID,
                    MessageType.CreateGroupResponce,
                    msg.Data));//имя тоже возвращаем
        }

        private async void DeleteGroupChat(Message msg)
        {
           var deleted=await _groupRepo.DeleteGroup(msg.FromID, msg.ChatID);
           _ = _clientNotifier.SendToUser(
                new Message(//генерируем ответ для юзера, который прислал запрос
                    Message.GenerateIDByTime(),
                    -1,
                    msg.ChatID,
                    msg.FromID,
                    MessageType.DeleteGroupResponce,
                    MessageUtils.BooleanToBytes(deleted)));//результат удаления
        }
    }
}
