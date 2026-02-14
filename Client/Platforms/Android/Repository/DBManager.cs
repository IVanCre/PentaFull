using MessageLib;
using Penta_ClientLib.DataStructs;
using Penta_ClientLib.Interfaces;
using Penta_ClientLib.Services;
using SQLite;


namespace Client.Platforms.Android.Repository
{
    internal class DBManager : ISettingsHolder, IChatHolder, IMessageHolder,IContactHolder
    {
        private string _dbFileName = $"WorkDB.db3";
        private SQLiteOpenFlags _creationFlags =
            SQLiteOpenFlags.ReadWrite |// open the database in read/write mode
            SQLiteOpenFlags.Create |// create the database if it doesn't exist
            SQLiteOpenFlags.SharedCache;// enable multi-threaded database access
        private string _dbFullPath;
        private SQLiteAsyncConnection _connection;

        public DBManager()
        {
            _dbFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _dbFileName);
            InitConnect();

            _connection.CreateTableAsync<Settings>();
            _connection.CreateTableAsync<Contact>();
            _connection.CreateTableAsync<Chat>();
            _connection.CreateTableAsync<UserInChat>();
            _connection.CreateTableAsync<MessageItem>();
        }
        private void InitConnect()
        {
            if (_connection == null)
                _connection = new SQLiteAsyncConnection(_dbFullPath, _creationFlags);
        }


        #region Settings
        public async Task<T> GetValueByName<T>(string paramName)
        {
            InitConnect();

            var finded = await _connection.Table<Settings>().FirstOrDefaultAsync(x => x.Name == paramName);
            if (finded != null)
                return (T)Convert.ChangeType(finded.Value, typeof(T));
            else
                return default;
        }
        public async void SetValueByName<T>(string paramName, T value)
        {
            InitConnect();

            var finded = await _connection.Table<Settings>().FirstOrDefaultAsync(x => x.Name == paramName);
            if (finded != null)
            {
                finded.Value = value.ToString();
                _ = _connection.UpdateAsync(finded);
            }
            else
                _ = _connection.InsertAsync(
                    new Settings()
                    {
                        Name = paramName,
                        Value = value.ToString()
                    });
        }
        #endregion


        #region Contacts
        public async Task<bool> AddContact(string userName, string connectID)
        {
            InitConnect();

            var inserted = await _connection.InsertAsync(
                new Contact()
                {
                    ID = ContactConverter.ExtractUserID(connectID),
                    UserName = userName,
                });

            return inserted == 1;
        }
        public async Task<string> GetContactIDByName(string userName)
        {
            var findedID = await GetUserIDByName(userName);
            if (findedID != -1)
                return  ContactConverter.ConvertUserIDToContactID(findedID);
            else
                return string.Empty;
        }
        public async Task<int> GetUserIDByName(string userName)
        {
            InitConnect();

            var finded = await _connection.Table<Contact>().FirstOrDefaultAsync(x => x.UserName == userName);
            if (finded != null)
                return finded.ID;
            else
                return -1;
        }
        public async Task<string> GetUserNameByContactID(string contactID)
        {
            InitConnect();

            int userID = ContactConverter.ExtractUserID(contactID);
            var finded = await _connection.Table<Contact>().FirstOrDefaultAsync(x => x.ID == userID);
            if (finded != null)
                return finded.UserName;
            else
                return string.Empty;
        }
        public async Task<List<ContactInfo>> GetAllContacts()
        {
            List<ContactInfo> list = new();
            var finded = await _connection.Table<Contact>().ToListAsync();
            foreach (var contact in finded)
                list.Add(new ContactInfo( contact.UserName,ContactConverter.ConvertUserIDToContactID(contact.ID)));

            return list;
        }
        public async Task<bool> DeleteByContactID(string contactID)
        {
            InitConnect();

            var deleted = await _connection.ExecuteAsync("delete from Contact where ID = ?", ContactConverter.ExtractUserID(contactID));
            return deleted == 1;
        }

        #endregion


        #region Chats        
        private async Task<bool> AddChat(int chatID, string chatName)
        {
            InitConnect();

            var added = await _connection.InsertAsync(
                new Chat()
                {
                    ID = chatID,
                    Name = chatName
                });

            return added == 1;
        }        
        public Task<bool> AddGroupChat(int chatID, string chatName)
        {
            return AddChat(chatID, chatName);
        }
        public async Task<int> CreatePrivateChat(string chatName)
        {
            int id = 0;
            InitConnect();
            var finded = await _connection.Table<Chat>().FirstOrDefaultAsync(x => x.Name == chatName);
            if (finded == null)
            {
                id = GenerateLocalIDByTime();
                if (!await AddChat(id, chatName))
                    id = 0;
            }
            else
                id = finded.ID;//значит уже есть созданный

            return id;
        }

        public async Task<int> GetChatID(string chatName)
        {
            var finded = await _connection.Table<Chat>().FirstOrDefaultAsync(x => x.Name == chatName);
            if (finded != null)
                return finded.ID;
            else
                return 0;
        }

        public async Task<bool> DeleteChat(int chatID)
        {
            InitConnect();

            var deleted = await _connection.ExecuteAsync("delete from Chat where ID = ?", chatID);
            await DeleteByChatID(chatID);
            return deleted == 1;
        }

        public async Task<bool> AddUserToChat(int userid, int chatID)
        {
            InitConnect();

            var inserted = await _connection.InsertAsync(
                new UserInChat()
                {
                    ContactId = userid,
                    ChatId = chatID
                });

            return inserted == 1;
        }

        public async Task<bool> RemoveUserFromChat(int userid, int chatID)
        {
            InitConnect();

            object[] param = { userid, chatID };
            var deleted = await _connection.ExecuteAsync("delete from UserInChat where ContactId = ? and ChatId = ?", param);

            return deleted == 1;
        }
        public async Task<List<ChatInfo>> GetAllChats()
        {
            List<ChatInfo> finded = new();
            var list = await _connection.Table<Chat>().ToListAsync();
            foreach (var item in list)
                finded.Add(new ChatInfo(item.ID, item.Name));

            return finded;
        }
        #endregion


        #region Messages
        public async Task<bool> SaveMessage(Message msg)
        {
            var result = await _connection.InsertAsync(
                 new MessageItem()
                 {
                     ID = msg.ID,
                     FromID = msg.FromID,
                     ChatID = msg.ChatID,
                     ToID = msg.ToID,
                     Type = msg.Type,
                     Data = msg.Data,
                     UtcTimestamp = DateTime.Now
                 });

            return result == 1;
        }
        public async Task<List<Message>> GetNonSended()
        {
            List<Message> result = new();
            var finded = await _connection.Table<MessageItem>().Where(x => x.IsSended == false).ToListAsync();
            foreach (var item in finded)
                result.Add(
                    new Message(
                        item.ID,
                        item.FromID,
                        item.ChatID,
                        item.ToID,
                        item.Type,
                        item.Data,
                        item.UtcTimestamp));

            return result;
        }
        public async Task<bool> DeleteByChatID(int chatID)
        {
            var deleted = await _connection.ExecuteAsync("delete from MessageItem where  ChatID = ?", chatID);
            return deleted > 0;
        }


        public async Task<List<Message>> GetMessagesByChat(int chatID, int maxLenCount)
        {
            List<Message> result = new();
            var finded = await _connection.Table<MessageItem>()
                .Where(x=>x.ChatID==chatID)
                .OrderBy(x=>x.UtcTimestamp)
                .Take(maxLenCount)
                .ToListAsync();

            foreach (var item in finded)
                result.Add(
                    new Message(
                        item.ID,
                        item.FromID,
                        item.ChatID,
                        item.ToID,
                        item.Type,
                        item.Data,
                        item.UtcTimestamp));

            return result;
        }
        #endregion

        private int GenerateLocalIDByTime()
        {
            int val = (int)(DateTime.Now - DateTime.Parse("01.01.2025")).TotalSeconds;
            if (val > 0)
                val = val * -1;//чтобы не путать ID сгенеренные серваком и свои локальные

            return val;
        }


    }
}
