using Message_Server.Interfaces;
using Message_Server.Services.Repositories.Models;
using MessageLib;
using Microsoft.AspNetCore.SignalR;


namespace Message_Server.Services.MessagesProcessors
{

    public class MessageProcessor(
        ILogWriter logger,
        IConnectionsRepository repo,
        IMessageSaver msgSaver,
        IGroupRepository groupRepo,
        IUserRepository userRepo) : IMessageProcessor
    {
        private IConnectionsRepository _connRepo = repo;
        private ILogWriter _logger = logger;
        private IMessageSaver _msgSaver = msgSaver;
        private IGroupRepository _groupRepo = groupRepo;
        private IUserRepository _userRepo = userRepo;


        public async Task ProcessingMessage(Message msg, IHubCallerClients connectedClients)
        {
            if(msg.IsUserToUser())
                await ProcessMessageForUser(msg, connectedClients);
            else 
                await ProcessMessageForGroup(msg, connectedClients);   
        }

        private async Task ProcessMessageForUser(Message msg, IHubCallerClients connectedClients)
        {
            var connectionID = _connRepo.GetConnectionID(msg.ToUserID);
            if (!string.IsNullOrEmpty(connectionID))
            {
                var client = connectedClients.Client(connectionID);
                if (client != null)
                    await client.SendAsync("RecieveMessage", msg);
            }
            else
            {
                _logger.SaveSystemInfo($"Получатель {msg.ToUserID} не в сети. Сохраняем");
                _msgSaver.Save(msg);//сохраняем в БД
            }
        }


        private async Task ProcessMessageForGroup(Message msg, IHubCallerClients connectedClients)
        {
            switch (msg.Type)
            {
                case MessageType.EnterToGroupResponce:       await EnterToGroupResponce(msg, connectedClients); break;
                case MessageType.LeaveGroupRequest:          await LeaveGroupRequest(msg, connectedClients); break;
                case MessageType.RemoveUserFromGroupRequest: await RemoveUserFromGroupRequest(msg, connectedClients); break;
                case MessageType.CreateGroupRequest:         await CreateGroup(msg); break;
                case MessageType.DeleteGroupRequest:         _groupRepo.DeleteGroup(msg.FromUserID, msg.GroupID); break;

                default: await SendMessageToAll(msg, connectedClients); break;
            }
        }
        private async Task EnterToGroupResponce(Message msg, IHubCallerClients connectedClients)
        {
            if (msg.GetDataLikeBoolean())//юзер согласен вступить в группу
            {
                if (await _groupRepo.AddUserToGroupAsync(msg.FromUserID, msg.GroupID))
                {
                    var name = _userRepo.FindUserNameByIDAsync(msg.FromUserID);
                    await SendMessageToAll(
                        new Message(//создаем новое сообщения для всех
                            -1,
                            msg.FromUserID,
                            msg.GroupID,
                            -1,
                            msg.Type,
                            MessageUtils.TextToBytes($"Юзер {name} присоединился к группе")),//оповещаем что мы добавились
                        connectedClients);
                }
            }
        }
        private async Task LeaveGroupRequest(Message msg, IHubCallerClients connectedClients)
        {
            if (await _groupRepo.RemoveUserFromGroupAsync(msg.FromUserID, msg.GroupID))
            {
                var name = _userRepo.FindUserNameByIDAsync(msg.FromUserID);
                await SendMessageToAll(
                    new Message(//создаем новое сообщения для всех
                        -1,
                        msg.FromUserID,
                        msg.GroupID,
                        -1,
                        msg.Type,
                        MessageUtils.TextToBytes($"Юзер {name} покинул группу")),//оповещаем что мы добавились
                    connectedClients);
            }
        }
        async Task RemoveUserFromGroupRequest(Message msg, IHubCallerClients connectedClients)
        {
            var finded = await _groupRepo.GetGroupByIDAsync(msg.GroupID);
            if (finded != null && finded.AdminGroupID == msg.FromUserID)
            {
                var userIDToDelete = msg.GetDataLikeInt();
                if (await _groupRepo.RemoveUserFromGroupAsync(userIDToDelete, msg.GroupID))
                {
                    var name = _userRepo.FindUserNameByIDAsync(msg.FromUserID);
                    await SendMessageToAll(
                        new Message(//создаем новое сообщения для всех
                            -1,
                            msg.FromUserID,
                            msg.GroupID,
                            -1,
                            msg.Type,
                            MessageUtils.TextToBytes($"Юзер {name} удален администратором группы")),//оповещаем что мы добавились
                        connectedClients);
                }
            }
        }
        private async Task<bool> CreateGroup(Message msg)
        {
           return await _groupRepo.CreatGroupAsync(msg.FromUserID, msg.GetDataLikeString());
        }



        private async Task SendMessageToAll(Message msg, IHubCallerClients connectedClients)
        {
            var finded = await _groupRepo.GetGroupByIDAsync(msg.GroupID);
            if (finded != null)
            {
                var usersID = finded.UserIDsInGroup();
                foreach (var userID in usersID)
                {
                    await ProcessMessageForUser(//для каждого юзера делаем отдельную копию сообщения
                        new Message(
                            -1,
                            msg.FromUserID,
                            msg.GroupID,
                            userID,
                            msg.Type,
                            msg.Data),
                        connectedClients);
                }
            }
        }
    }
}
