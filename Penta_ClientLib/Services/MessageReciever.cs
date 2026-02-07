using Penta_ClientLib.Interfaces;
using MessageLib;

namespace Penta_ClientLib.Services
{
    internal class MessageReciever: IMessageReciever
    {
        private IChatHolder _chatHolder;
        private IWebClient _messListener;
        private IMessageHolder _messHolder;

        public event ChatChanged CreatedNewChat;
        public event ChatChanged ChatDeleted;
        public event ChatUserListChanged UserAdded;
        public event ChatUserListChanged UserRemoved;
        public event NewMessageInChat MessageAddedToChat;
        public event InvitedToChat RecieveInvite;
        public event AccountDeleted AccountDeleted;


        public MessageReciever(
            IWebClient messListener,
            IChatHolder chatProvider,
            IMessageHolder messHolder
            )
        {
            _messListener = messListener;
            _messListener.RecievedMessage += ProcessResponce;
            _chatHolder = chatProvider;
            _messHolder = messHolder;
        }        
        
        private async void ProcessResponce(Message msg)//просматриваем ответы от сервера
        {
            await _messHolder.SaveMessage(msg);//сразу сохраняем

            switch (msg.Type)
            {
                case MessageType.InviteToGroupResponce:         await ProcessInviteUserToGroupResponce(msg); break;
                case MessageType.RemoveUserFromGroupResponce:   await ProcessDeleteUserFromGroupResponce(msg); break;
                case MessageType.CreateGroupResponce:           await ProcessCreateGroupChatResponce(msg);break;
                case MessageType.DeleteGroupResponce:           await ProcessDeleteGroupResponce(msg);break;
                case MessageType.InviteToGroupRequest:          RecieveInvite?.Invoke(msg); break;
                case MessageType.DeleteSelfAccountResponce:     AccountDeleted?.Invoke(); ; break;

                case MessageType.Text:
                case MessageType.Picture:
                case MessageType.Voice:                         await ProcessMessageFromUser(msg);break;
            }
        }
        private async Task ProcessCreateGroupChatResponce(Message msg)//ответ на запрос
        {
            if (msg.ChatID!=-1)//значит сервак успешно создал
            {
                var chatName = msg.GetDataLikeString();
                if(await _chatHolder.AddGroupChat(msg.ChatID,chatName ))
                    CreatedNewChat?.Invoke(msg.ChatID,chatName);
            }
        }
        private async Task ProcessInviteUserToGroupResponce(Message msg)
        {
            if (await _chatHolder.AddUserToChat(msg.FromID,msg.ChatID ))//запоминаем юзера, который добавился в чат
                UserAdded?.Invoke(msg.ChatID, msg.FromID);//информируем наверх, что кто-то присоединился к чату,в котором есть мы
        }

        private async Task ProcessDeleteUserFromGroupResponce(Message msg)
        {
            var userIDToDelete = msg.GetDataLikeInt();
            if (await _chatHolder.RemoveUserFromChat(userIDToDelete, msg.ChatID))
            {
                UserRemoved?.Invoke(msg.ChatID, userIDToDelete);
            }
        }        
        
        private async Task ProcessDeleteGroupResponce(Message msg)
        {
            if (await _chatHolder.DeleteChat(msg.ChatID))
            {
                ChatDeleted?.Invoke(msg.ChatID,"");
            }
        }        
        private async Task ProcessMessageFromUser(Message msg)
        {
            if(msg.ChatID==-1)//личное сообщение
            {
                var userConnectionID = ContactConverter.ConvertUserIDToContactID(msg.FromID);
                var chatID= await _chatHolder.CreatePrivateChat(userConnectionID);//чат называется именем собеседника
                if (chatID != 0)//значит чат создан
                {
                    CreatedNewChat?.Invoke(chatID, userConnectionID);
                    UserAdded?.Invoke(chatID, msg.FromID);
                }
            }
            else//групповое сообщение
            {
                //тут уже есть чат
                var added =await _chatHolder.AddUserToChat(msg.FromID, msg.ChatID);// прикрепляем нового юзера к чату(чтобы видеть его у себя)
                if(added)
                    UserAdded?.Invoke(msg.ChatID, msg.FromID);
            }

            MessageAddedToChat?.Invoke(msg.ChatID,msg);
        }

    }
}
