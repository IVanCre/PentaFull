using MessageLib;
using Penta_ClientLib.DataStructs;
using Penta_ClientLib.Interfaces;

namespace ConsoleClient_Tests
{
    /// <summary>
    /// Заглушка для замены БД(для тестов нескольких клиентов на одной машине)
    /// </summary>
    internal class MemoryDB : ISettingsHolder, IChatHolder, IMessageHolder,IContactHolder
    {
        // Внутренние словари
        private readonly Dictionary<string, object> _settings = new();

        private readonly Dictionary<string, int> _contacts = new();
 
        private readonly Dictionary<int, string> _chats = new();
        private readonly Dictionary<int, List<int>> _userInChats = new();

        private readonly List< Message> _messages = new();
        private int _nextChatId = 0; // генерирются отрицательными, чтобы не накладываться на групповые(положительные)



        #region Settings
        public async Task<T> GetValueByName<T>(string paramName)
        {
            if (_settings.TryGetValue(paramName, out var value))
            {
                return await Task.FromResult((T)Convert.ChangeType(value, typeof(T)));
            }
            return await Task.FromResult(default(T));
        }

        public async void SetValueByName<T>(string paramName, T value)
        {
            if (_settings.ContainsKey(paramName))
            {
                _settings[paramName] = value;
            }
            else
            {
                _settings.Add(paramName, value);
            }
            await Task.CompletedTask;
        }
        #endregion



        #region Chats
        public async Task<bool> AddGroupChat(int chatID, string chatName)
        {
            if (!_chats.ContainsKey(chatID))
            { 
                _chats.Add(chatID,chatName );
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        public async Task<int> CreatePrivateChat(string chatName)
        {
            if(!_chats.Values.Contains(chatName))
            {
                int id = --_nextChatId;
                _chats.Add(id, chatName);
                return await Task.FromResult(id);
            }
            else
                return 0;
        }

        public async Task<int> GetChatID(string chatName)
        {
            int id = -1;
            foreach(var key in _chats.Keys)
            {
                if (_chats[key] == chatName)
                {
                    id = key;
                    break;
                }
            }
            return await Task.FromResult(id);
        }

        public async Task<bool> DeleteChat(int chatID)
        {
            return await Task.FromResult(_chats.Remove(chatID));
        }


        public async Task<bool> AddUserToChat(int userID, int chatID)
        {
            if (_userInChats.ContainsKey(chatID))
            {
                if (!_userInChats[chatID].Contains(userID))//юзера в чате нет
                {
                    _userInChats[chatID].Add(userID);
                    return await Task.FromResult(true);
                }
                else//значит юзер уже есть в чате
                    return await Task.FromResult(false);
            }
            else//чата вообще еще нет
            {
                var list = new List<int>();
                list.Add(userID);
                _userInChats.Add(chatID, list);
                return await Task.FromResult(true);
            }
        }

        public async Task<bool> RemoveUserFromChat(int userID, int chatID)
        {
            if (_userInChats.ContainsKey(chatID))
            {
                _userInChats[chatID].Remove(userID);
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        public async Task<List<ChatInfo>> GetAllChats()
        {
            List<ChatInfo> result = new();
            foreach(var key in _chats.Keys)
            {
                result.Add(new ChatInfo(key, _chats[key]));
            }

            return await Task.FromResult(result);
        }
        #endregion



        #region Messages
        public async Task<bool> SaveMessage(Message msg)
        {
            _messages.Add(msg);
            return await Task.FromResult(true);
        }

        public async Task<List<Message>> GetNonSended()
        {
//заглушка
            return await Task.FromResult(new List<Message>());
        }

        public async Task<bool> DeleteByChatID(int chatID)
        {
            var toRemove = _messages.Where(m => m.ChatID == chatID).ToList();
            foreach (var id in toRemove)
                _messages.Remove(id);

            return await Task.FromResult(true);
        }
        #endregion        


        public Task<List<Message>> GetMessagesByChat(int chatID, int maxLastMessageCount)
        {
            return Task.FromResult(new List<Message>());
        }

        public Task<bool> AddContact(string name, string contactID)
        {
            return Task.FromResult(false);
        }

        public Task<List<ContactInfo>> GetAllContacts()
        {
            return Task.FromResult(new List<ContactInfo>());
        }

        public Task<string> GetUserNameByContactID(string contactID)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<string> GetContactIDByName(string name)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<int> GetUserIDByName(string name)
        {
            return Task.FromResult(0);
        }

        public Task<bool> DeleteByContactID(string contactID)
        {
            return Task.FromResult(false);
        }
    }
}

