using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageLib;

namespace Messaga_Client.Services.Chats
{
    internal class Chat
    {
        public string ChatName;
        public List<string> users = new();
        public List<Message> _messageList { get;} = new();

        public void AddMessage(Message message)
        {
            _messageList.Add(message);
        }

        public void AddUser(string userName)
        {
            users.Add(userName);
        }
    }


    public interface IChatsProvider
    {
        Task<List<string>> GetAvailableChatsNames();

        Task<Chat> CreateNewChat(string chatName);//создает сущность чата(список из Message)
        Task<bool> FullDeleteChatByName(string chatName);//удаляет чат и все сообщения связанные с ним
    }


    internal class ChatsManager : IChatsProvider
    {
        public Task<List<string>> GetAvailableChatsNames()
        {
            throw new NotImplementedException();
        }

        public Task<Chat> GetChatByName(string chatName)
        {
            throw new NotImplementedException();
        }        
        
        public Task<bool> FullDeleteChatByName(string chatName)
        {
            throw new NotImplementedException();
        }
    }
}
