using Message_Server.Interfaces;
using MessageLib;


namespace Message_Server.Services.MessagesProcessors
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
                case MessageType.EnterToGroupResponce:       await EnterToGroupResponce(msg); break;
                case MessageType.LeaveGroupRequest:          await LeaveGroupRequest(msg); break;
                case MessageType.RemoveUserFromGroupRequest: await RemoveUserFromGroupRequest(msg); break;

                case MessageType.CreateGroupRequest:         await CreateGroup(msg); break;
                case MessageType.DeleteGroupRequest:         _groupRepo.DeleteGroup(msg.FromID, msg.GroupID); break;

                default: await _clientNotifier.SendToGroup(msg); break;
            }
        }


        private async Task EnterToGroupResponce(Message msg)
        {
            if (msg.GetDataLikeBoolean())//юзер согласен вступить в группу
            {
                if (await _groupRepo.AddUserToGroupAsync(msg.FromID, msg.GroupID))
                {
                    var name = _userRepo.FindUserNameByIDAsync(msg.FromID);
                    await _clientNotifier.SendToGroup(
                        new Message(//создаем новое сообщения для всех
                            -1,
                            msg.FromID,
                            msg.GroupID,
                            -1,
                            msg.Type,
                            MessageUtils.TextToBytes($"Юзер {name} присоединился к группе")));//оповещаем что мы добавились
                }
            }
        }
        private async Task LeaveGroupRequest(Message msg)
        {
            if (await _groupRepo.RemoveUserFromGroupAsync(msg.FromID, msg.GroupID))
            {
                var name = _userRepo.FindUserNameByIDAsync(msg.FromID);
                await _clientNotifier.SendToGroup(
                    new Message(//создаем новое сообщения для всех
                        -1,
                        msg.FromID,
                        msg.GroupID,
                        -1,
                        msg.Type,
                        MessageUtils.TextToBytes($"Юзер {name} покинул группу")));//оповещаем что мы добавились

            }
        }
        private async Task RemoveUserFromGroupRequest(Message msg)
        {
            var finded = await _groupRepo.GetGroupByIDAsync(msg.GroupID);
            if (finded != null && finded.AdminGroupID == msg.FromID)
            {
                var userIDToDelete = msg.GetDataLikeInt();
                if (await _groupRepo.RemoveUserFromGroupAsync(userIDToDelete, msg.GroupID))
                {
                    var name = _userRepo.FindUserNameByIDAsync(msg.FromID);
                    await _clientNotifier.SendToGroup(
                        new Message(//создаем новое сообщения для всех
                            -1,
                            msg.FromID,
                            msg.GroupID,
                            -1,
                            msg.Type,
                            MessageUtils.TextToBytes($"Юзер {name} удален администратором группы")));
                }
            }
        }
        private async Task<bool> CreateGroup(Message msg)
        {
           return await _groupRepo.CreatGroupAsync(msg.FromID, msg.GetDataLikeString());
        }
    }
}
