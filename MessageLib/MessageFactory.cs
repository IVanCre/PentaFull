

namespace MessageLib
{
    public static class MessageFactory
    {
        public static Message CreateGroupChat_Request(int senderUserID, string chatName)
        {
            return new Message(
                    Message.GenerateIDByTime(),
                    senderUserID,
                    -1,
                    -1,
                    MessageType.CreateGroupRequest,
                    MessageUtils.TextToBytes(chatName),
                    DateTimeOffset.UtcNow);
        }
        public static Message CreateGroupChat_Response(int chatID,Message request)
        {
           return new Message(//генерируем ответ для юзера, который прислал запрос
                    Message.GenerateIDByTime(),
                    -1,
                    chatID,//его определяет сервер при создании у себя
                    request.FromID,
                    MessageType.CreateGroupResponce,
                    request.Data,
                    DateTimeOffset.UtcNow);//имя тоже возвращаем(если успешно)
        }


        public static Message DeleteGroupChat_Request(int senderUserID, int chatID) 
        {
            return new Message(
                    Message.GenerateIDByTime(),
                    senderUserID,
                    chatID,
                    -1,
                    MessageType.DeleteGroupRequest,
                    null,
                    DateTimeOffset.UtcNow);
        }
        public static Message DeleteGroupChat_Response(Message request, int userRecieverID)
        {
            return new Message(
                         request.ID,
                         -1,
                         request.ChatID,
                         userRecieverID,
                         MessageType.DeleteGroupResponce,
                         null,
                         DateTimeOffset.UtcNow);
        }


        //1.userSender->server
        public static Message AddUserToGroupChat_Request(int senderUserID, int chatID, int userToAddedID)
        {
            return new Message(
                    Message.GenerateIDByTime(),
                    senderUserID,//кто добавляет
                    chatID,//куда добавляет
                    userToAddedID,//кого добавляют
                    MessageType.AddToGroupRequest,
                    null,
                    DateTimeOffset.UtcNow);
        }
        //2.1server->all users (for each in groupChat)
        public static Message AddUserToGroupChat_Response(Message request,int recieverUserID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                             request.ID,
                             request.FromID,//кто добавляет
                             request.ChatID,//куда добавляют
                             recieverUserID,//кто получит это сообщение
                             MessageType.AddUserToGroupResponse,
                             null,
                             DateTimeOffset.UtcNow);
        }
        //2.2 server->recieverUserID
        public static Message UserAddedToGroupChat_ServerResponse(int recieverUserID,int chatID, string chatName)
        {
            return new Message(//создаем новое сообщения для добавленного юзера
                 Message.GenerateIDByTime(),
                 -1,
                 chatID,//куда добавляют
                 recieverUserID,//кого добавляют
                 MessageType.UserAddedToGroupResponse,
                 MessageUtils.TextToBytes(chatName),//имя чата
                 DateTimeOffset.UtcNow);
        }



        //1.userSended->server
        public static Message DeleteUserFromGroupChat_Request(int senderUserID,int userForDeleteID,int chatID)
        {
            return new Message(
                Message.GenerateIDByTime(),
                senderUserID,
                chatID,
                userForDeleteID,
                MessageType.RemoveUserFromGroupRequest,
                null,
                DateTimeOffset.UtcNow);
        }
        //2.server->all users
        public static Message DeleteUserFromGroupChat_Response(Message request, int recieverUserID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                 request.ID,
                 -1,
                 request.ChatID,
                 recieverUserID,
                 MessageType.RemoveUserFromGroupResponce,
                 MessageUtils.IntToBytes(request.ToID),
                 DateTimeOffset.UtcNow);
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
                request.UtcTimestamp);
        }

        public static Message UserToUser(int senderUserID,int recieverUserID, MessageType type, byte[] data)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateIDByTime(),
                senderUserID,
                -1,
                recieverUserID,
                type,
                data,
                DateTimeOffset.UtcNow);
        }
        public static Message UserToGroupChat(int senderUserID, int chatID, MessageType type, byte[] data)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateIDByTime(),
                senderUserID,
                chatID,
                -1,
                type,
                data,
                DateTimeOffset.UtcNow);
        }

        public static Message DeleteAccountRequest(int senderUserID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateIDByTime(),
                senderUserID,
                -1,
                -1,
                MessageType.DeleteSelfAccountRequest,
                null,
                DateTimeOffset.UtcNow);
        }
        public static Message DeleteAccountResponce(Message request)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateIDByTime(),
                request.FromID,
                -1,
                request.FromID,
                MessageType.DeleteSelfAccountRequest,
                null,
                DateTimeOffset.UtcNow);
        }

        public static Message CreateNotify(int recieverID, string message)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateIDByTime(),
                -1,
                -1,
                recieverID,
                MessageType.SystemNotify,
                MessageUtils.TextToBytes(message),
                DateTimeOffset.UtcNow);
        }
    }
}
