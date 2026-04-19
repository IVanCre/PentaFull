

namespace MessageLib
{
    public static class MessageFactory
    {
        public static Message CreateGroupChat_Request(int senderUserID, string chatName)
        {

            return new Message(
                    Message.GenerateLocalIDByTime(),
                    senderUserID,
                    -1,
                    -1,
                    MessageType.CreateGroupRequest,
                    MessageUtils.TextToBytes(chatName),
                    DateTimeOffset.UtcNow,
                    false);
        }
        public static Message CreateGroupChat_Response(int chatID,Message request)
        {
           return new Message(//генерируем ответ для юзера, который прислал запрос
                    Message.GenerateLocalIDByTime(),
                    -1,
                    chatID,//его определяет сервер при создании у себя
                    request.FromID,
                    MessageType.CreateGroupResponce,
                    request.Data,
                    DateTimeOffset.UtcNow,
                    false);//имя тоже возвращаем(если успешно)
        }


        public static Message CreateDeleteGroupChat_Request(int senderUserID, int chatID) 
        {
            return new Message(
                    Message.GenerateLocalIDByTime(),
                    senderUserID,
                    chatID,
                    -1,
                    MessageType.DeleteGroupRequest,
                    null,
                    DateTimeOffset.UtcNow,
                    false);
        }
        public static Message CreateDeleteGroupChat_Response(Message request, int userRecieverID)
        {
            return new Message(
                         request.ID,
                         -1,
                         request.ChatID,
                         userRecieverID,
                         MessageType.DeleteGroupResponce,
                         null,
                         DateTimeOffset.UtcNow,
                         false);
        }


        //1.userSender->server
        public static Message CreateAddUserToGroupChat_Request(int senderUserID, int chatID, int userToAddedID)
        {
            return new Message(
                    Message.GenerateLocalIDByTime(),
                    senderUserID,//кто добавляет
                    chatID,//куда добавляет
                    userToAddedID,//кого добавляют
                    MessageType.AddToGroupRequest,
                    null,
                    DateTimeOffset.UtcNow,
                    false);
        }
        //2.1server->all users (for each in groupChat)
        public static Message CreateAddUserToGroupChat_Response(Message request,int recieverUserID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                             request.ID,
                             request.FromID,//кто добавляет
                             request.ChatID,//куда добавляют
                             recieverUserID,//кто получит это сообщение
                             MessageType.AddUserToGroupResponse,
                             null,
                             DateTimeOffset.UtcNow,
                             false);
        }
        //2.2 server->recieverUserID
        public static Message CreateUserAddedToGroupChat_ServerResponse(int recieverUserID,int chatID, string chatName)
        {
            return new Message(//создаем новое сообщения для добавленного юзера
                 Message.GenerateLocalIDByTime(),
                 -1,
                 chatID,//куда добавляют
                 recieverUserID,//кого добавляют
                 MessageType.UserAddedToGroupResponse,
                 MessageUtils.TextToBytes(chatName),//имя чата
                 DateTimeOffset.UtcNow,
                 false);
        }



        //1.userSended->server
        public static Message CreateDeleteUserFromGroupChat_Request(int senderUserID,int userForDeleteID,int chatID)
        {
            return new Message(
                Message.GenerateLocalIDByTime(),
                senderUserID,
                chatID,
                userForDeleteID,
                MessageType.RemoveUserFromGroupRequest,
                null,
                DateTimeOffset.UtcNow,
                false);
        }
        //2.server->all users
        public static Message CreateDeleteUserFromGroupChat_Response(Message request, int recieverUserID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                 request.ID,
                 -1,
                 request.ChatID,
                 recieverUserID,
                 MessageType.RemoveUserFromGroupResponce,
                 MessageUtils.IntToBytes(request.ToID),
                 DateTimeOffset.UtcNow,
                 false);
        }

        public static Message CreateResponseForGroupMember(Message request, int recieverUserID)
        {
            return new Message(
                request.ID,
                request.FromID,
                request.ChatID,
                recieverUserID,
                request.Type,
                request.Data,
                request.UtcTimestamp,
                false);
        }

        public static Message CreateUserToUser(int senderUserID,int recieverUserID, MessageType type, byte[] data, long? messageID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                messageID==null ? Message.GenerateLocalIDByTime(): messageID.Value, //внешний айдишник может быть передан из связанной сущности
                senderUserID,
                -1,
                recieverUserID,
                type,
                data,
                DateTimeOffset.UtcNow,
                false);
        }
        public static Message CreateUserToGroupChat(int senderUserID, int chatID, MessageType type, byte[] data, long? messageID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                messageID == null ? Message.GenerateLocalIDByTime() : messageID.Value,
                senderUserID,
                chatID,
                -1,
                type,
                data,
                DateTimeOffset.UtcNow,
                false);
        }

        public static Message CreateDeleteAccountRequest(int senderUserID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateLocalIDByTime(),
                senderUserID,
                -1,
                -1,
                MessageType.DeleteSelfAccountRequest,
                null,
                DateTimeOffset.UtcNow,
                false);
        }
        public static Message CreateDeleteAccountResponce(Message request)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateLocalIDByTime(),
                request.FromID,
                -1,
                request.FromID,
                MessageType.DeleteSelfAccountRequest,
                null,
                DateTimeOffset.UtcNow,
                false);
        }

        public static Message CreateNotify(int recieverID, string message)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateLocalIDByTime(),
                -1,
                -1,
                recieverID,
                MessageType.SystemNotify,
                MessageUtils.TextToBytes(message),
                DateTimeOffset.UtcNow,
                false);
        }


        public static Message StartObserveUserInSystem(int senderID,int chatID, int observerUserID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateLocalIDByTime(),
                senderID,
                chatID,
                observerUserID,
                MessageType.StartObservRecieverConnect,
                null,
                DateTimeOffset.UtcNow,
                false);
        }
        public static Message UserInSystemState(int chatID,int observerUserID, int recieverID, bool state)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateLocalIDByTime(),
                observerUserID,//кого отслеживаем
                chatID,//какой чат отслеживает
                recieverID,//кто отслеживает
                MessageType.UserInSystemState,
                MessageUtils.BoolToBytes(state),
                DateTimeOffset.UtcNow,
                false);
        }
        public static Message EndObserveUserInSystem(int senderID, int chatID, int observerUserID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateLocalIDByTime(),
                senderID,
                chatID,
                observerUserID,
                MessageType.EndObservRecieverConnect,
                null,
                DateTimeOffset.UtcNow,
                false);
        }
    }
}
