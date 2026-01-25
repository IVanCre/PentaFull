

using Microsoft.VisualBasic;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    MessageUtils.TextToBytes(chatName));
        }
        public static Message CreateGroupChat_Response(int chatID,Message request)
        {
           return new Message(//генерируем ответ для юзера, который прислал запрос
                    Message.GenerateIDByTime(),
                    -1,
                    chatID,//его определяет сервер при создании у себя
                    request.FromID,
                    MessageType.CreateGroupResponce,
                    request.Data);//имя тоже возвращаем(если успешно)
        }


        public static Message DeleteGroupChat_Request(int senderUserID, int chatID) 
        {
            return new Message(
                    Message.GenerateIDByTime(),
                    senderUserID,
                    chatID,
                    -1,
                    MessageType.DeleteGroupRequest,
                    null);
        }
        public static Message DeleteGroupChat_Response(Message request, int userRecieverID)
        {
            return new Message(
                         Message.GenerateIDByTime(),
                         -1,
                         request.ChatID,
                         userRecieverID,
                         MessageType.DeleteGroupResponce,
                         null);
        }


        //1.userSender->userReciever  
        public static Message InviteUserToGroupChat_Request(int senderUserID, int chatID, int userRecieverID)
        {
            return new Message(
                    Message.GenerateIDByTime(),
                    senderUserID,
                    chatID,
                    userRecieverID,
                    MessageType.InviteToGroupRequest,
                    null);
        }
        //2.userReciever->server
        public static Message InviteUserToGroupChat_UserResponse(int senderUserID, int chatID, bool acceptInvite)
        {
            return new Message(
                            Message.GenerateIDByTime(),
                            senderUserID,
                            chatID,
                            -1,
                            MessageType.InviteToGroupResponce,
                            MessageUtils.BooleanToBytes(acceptInvite));
        }
        //3.server->all users (for each in groupChat)
        public static Message InviteUserToGroupChat_ServerResponse(Message request,int recieverUserID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                             Message.GenerateIDByTime(),
                             request.FromID,//этот идентификатор уже принимающая сторона отобразит как connectID
                             request.ChatID,
                             recieverUserID,
                             MessageType.InviteToGroupResponce,
                             null);
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
                null);
        }
        //2.server->all users
        public static Message DeleteUserFromGroupChat_Response(Message request, int recieverUserID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                 Message.GenerateIDByTime(),
                 -1,
                 request.ChatID,
                 recieverUserID,
                 MessageType.RemoveUserFromGroupResponce,
                 MessageUtils.IntToBytes(request.ToID));
        }



        public static Message UserToUser(int senderUserID,int recieverUserID, MessageType type, byte[] data)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateIDByTime(),
                senderUserID,
                -1,
                recieverUserID,
                type,
                data);
        }
        public static Message UserToGroupChat(int senderUserID, int chatID, MessageType type, byte[] data)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateIDByTime(),
                senderUserID,
                chatID,
                -1,
                type,
                data);
        }

        public static Message DeleteAccountRequest(int senderUserID)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
                Message.GenerateIDByTime(),
                senderUserID,
                -1,
                -1,
                MessageType.DeleteSelfAccountRequest,
                null);
        }
        public static Message DeleteAccountResponce(Message request)
        {
            return new Message(//создаем новое сообщения для всех кто в группе
            Message.GenerateIDByTime(),
            request.FromID,
            -1,
            request.FromID,
            MessageType.DeleteSelfAccountRequest,
            null);
        }
    }
}
