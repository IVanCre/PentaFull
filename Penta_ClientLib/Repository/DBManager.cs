using MessageLib;
using Penta_ClientLib.DataStructs;
using Penta_ClientLib.Interfaces;
using Penta_ClientLib.Services;
using SQLite;


namespace Penta_ClientLib.Repository
{
    internal class DBManager :  ISettingsHolder, IChatHolder, IMessageHolder,IContactHolder
    {
        private string _dbFileName = $"PentaClientDB.db3";
        private SQLiteOpenFlags _creationFlags =
            SQLiteOpenFlags.ReadWrite |// open the database in read/write mode
            SQLiteOpenFlags.Create |// create the database if it doesn't exist
            SQLiteOpenFlags.SharedCache;// enable multi-threaded database access
        private string _dbFullPath;
        private SQLiteAsyncConnection _connection;
        private ILogger _logger;

        public DBManager(ILogger logger)
        {
            _logger = logger;
            _dbFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _dbFileName);//в винде: C:\Users\UsernameX\AppData\Roaming
            InitConnect();

            _connection.CreateTableAsync<SettingsEntity>();
            _connection.CreateTableAsync<ContactEntity>();
            _connection.CreateTableAsync<ChatEntity>();
            _connection.CreateTableAsync<UserInChatEntity>();
            _connection.CreateTableAsync<MessageItemEntity>();
            _connection.CreateTableAsync<AmGroupAdminEntity>();
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

            var finded = await _connection.Table<SettingsEntity>().FirstOrDefaultAsync(x => x.Name == paramName);
            if (finded != null)
                return (T)Convert.ChangeType(finded.Value, typeof(T));
            else
                return default(T);
        }
        public async Task SetValueByName<T>(string paramName, T value)
        {
            InitConnect();
            try
            {
                var finded = await _connection.Table<SettingsEntity>().FirstOrDefaultAsync(x => x.Name == paramName);
                if (finded != null)
                {
                    finded.Value = value.ToString();
                    await _connection.UpdateAsync(finded);
                }
                else
                    await _connection.InsertAsync(
                        new SettingsEntity()
                        {
                            Name = paramName,
                            Value = value.ToString()
                        });
            }
            catch(Exception e)
            {
                _logger.SaveMessage(LogEventType.Error,$"DBManager exception: {e.Message}");
            }
        }
        #endregion


        #region Contacts
        public async Task<bool> AddContact(string userName, string connectID)
        {
            InitConnect();
            try
            {
                var inserted = await _connection.InsertAsync(
                    new ContactEntity()
                    {
                        ID = ContactConverter.ExtractUserID(connectID),
                        UserName = userName,
                    });

                return inserted == 1;
            }
            catch(Exception e)
            {
                _logger.SaveMessage(LogEventType.Error, $"DBManager exception: {e.Message}");
                return false;
            }
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

            var finded = await _connection.Table<ContactEntity>().FirstOrDefaultAsync(x => x.UserName == userName);
            if (finded != null)
                return finded.ID;
            else
                return -1;
        }
        public async Task<string> GetUserNameByID(int userID)
        {
            InitConnect();

            var finded = await _connection.Table<ContactEntity>().FirstOrDefaultAsync(x => x.ID == userID);
            if (finded != null)
                return finded.UserName;//имя из контакта
            else
                return string.Empty;
        }
        public async Task<List<ContactInfo>> GetAllContacts()
        {
            List<ContactInfo> list = new();
            var finded = await _connection.Table<ContactEntity>().OrderBy(x=>x.UserName).ToListAsync();
            foreach (var contact in finded)
                list.Add(new ContactInfo( contact.UserName,ContactConverter.ConvertUserIDToContactID(contact.ID)));

            return list;
        }
        public async Task<bool> DeleteByName(string userName)
        {
            InitConnect();
            try
            {
                var deleted = await _connection.Table<ContactEntity>()
                    .Where(c => c.UserName == userName)
                    .DeleteAsync();

                return deleted == 1;
            }
            catch (Exception e)
            {
                _logger.SaveMessage(LogEventType.Error, $"DBManager exception: {e.Message}");
                return false;
            }
        }

        #endregion


        #region Chats        
        private async Task<bool> CreateChat(int chatID, string chatName, ChatType chatType)
        {
            InitConnect();
            try
            {
                var finded = await _connection.Table<ChatEntity>().FirstOrDefaultAsync(x => x.ID == chatID);
                if (finded == null)
                {
                    var added = await _connection.InsertAsync(
                        new ChatEntity()
                        {
                            ID = chatID,
                            Name = chatName,
                            ChatType = chatType
                        });

                    return added == 1;
                }
                else//значит чат уже есть
                    return true;
            }
            catch (Exception e)
            {
                _logger.SaveMessage(LogEventType.Error, $"DBManager exception: {e.Message}");
                return false;
            }
        }        
        public async Task<bool> CreateGroupChat(int chatID, string chatName, bool requestFromAdminGroup)
        {
            try
            {
                bool result = await CreateChat(chatID, chatName, ChatType.Group);
                if (result && requestFromAdminGroup)
                {
                    await _connection.InsertAsync(
                        new AmGroupAdminEntity()
                        {
                            ChatID = chatID,
                        });
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.SaveMessage(LogEventType.Error, $"DBManager exception: {e.Message}");
                return false;
            }
        }


        public async Task<bool> AmCreatedThisGroupChat(int chatID)
        {
            InitConnect();

            var finded = await _connection.Table<AmGroupAdminEntity>().FirstOrDefaultAsync(x => x.ChatID == chatID);
            return finded != null;//значит указанный чат есть в таблице, значит его создание инциировали мы(мы -хозяинГруппы)
        }
        public async Task<int> CreatePrivateChat(string chatName)
        {
            int id = 0;
            InitConnect();
            try
            {
                var finded = await _connection.Table<ChatEntity>().FirstOrDefaultAsync(x => x.Name == chatName);
                if (finded == null)
                {
                    id = GenerateLocalIDByTime();
                    if (!await CreateChat(id, chatName, ChatType.Private))
                        id = 0;
                }
                else
                    id = finded.ID;//значит уже есть созданный

            }
            catch (Exception e)
            {
                _logger.SaveMessage(LogEventType.Error, $"DBManager exception: {e.Message}");
                id = 0;
            }
            return id;
        }
        public async Task<int> CreateReadOnlyChat(string chatName)
        {
            int id = 0;
            InitConnect();
            try
            {
                var finded = await _connection.Table<ChatEntity>().FirstOrDefaultAsync(x => x.Name == chatName);
                if (finded == null)
                {
                    id = GenerateLocalIDByTime();
                    if (!await CreateChat(id, chatName, ChatType.ReadOnly))
                        id = 0;
                }
                else
                    id = finded.ID;//значит уже есть созданный
            }
            catch (Exception e)
            {
                _logger.SaveMessage(LogEventType.Error, $"DBManager exception: {e.Message}");
                id = 0;
            }
            return id;
        }

        public async Task<int> GetChatIDByName(string chatName)
        {
            InitConnect();

            var finded = await _connection.Table<ChatEntity>().FirstOrDefaultAsync(x => x.Name == chatName);
            if (finded != null)
                return finded.ID;
            else
                return 0;
        }

        public async Task<bool> DeleteChat(int chatID)
        {
            InitConnect();
            try
            {
                await _connection.Table<MessageItemEntity>()
                    .Where(c => c.ChatID == chatID)
                    .DeleteAsync();

                var deleted = await _connection.Table<ChatEntity>()
                    .Where(c => c.ID == chatID)
                    .DeleteAsync();

                if (deleted == 1)
                    await _connection.Table<AmGroupAdminEntity>()
                        .Where(c => c.ChatID == chatID)
                        .DeleteAsync();

                return deleted == 1;
            }
            catch (Exception e)
            {
                _logger.SaveMessage(LogEventType.Error, $"DBManager exception: {e.Message}");
                return false;
            }
        }

        public async Task<bool> TryAddUserToChat(int userid, int chatID)
        {
            InitConnect();
            try
            {
                var findUserCopy = await _connection.Table<UserInChatEntity>().FirstOrDefaultAsync(x => x.UserId == userid && x.ChatId == chatID);
                if (findUserCopy == null)
                {
                    var inserted = await _connection.InsertAsync(
                         new UserInChatEntity()
                         {
                             UserId = userid,
                             ChatId = chatID
                         });

                    return inserted == 1;
                }
                else
                    return false;//юзер уже есть
            }
            catch (Exception e)
            {
                _logger.SaveMessage(LogEventType.Error, $"DBManager exception: {e.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveUserFromChat(int userid, int chatID)
        {
            InitConnect();
            try
            {
                var deleted = await _connection.Table<UserInChatEntity>()
                    .Where(c => c.UserId == userid && c.ChatId == chatID)
                    .DeleteAsync();

                return deleted == 1;
            }
            catch (Exception e)
            {
                _logger.SaveMessage(LogEventType.Error, $"DBManager exception: {e.Message}");
                return false;
            }
        }
        public async Task<List<ChatInfo>> GetAllChats()
        {
            List<ChatInfo> finded = new();
            var list = await _connection.Table<ChatEntity>().ToListAsync();
            foreach (var item in list)
                finded.Add(new ChatInfo(item.ID, item.Name,item.ChatType,item.HaveUnreaded));

            return finded;
        }

        public async Task<ChatInfo> GetChatByID(int chatID)
        {
            ChatInfo findedChat = null;
            var finded = await _connection.Table<ChatEntity>().FirstOrDefaultAsync(x => x.ID == chatID);
            if (finded != null)
                findedChat = new ChatInfo(finded.ID, finded.Name, finded.ChatType, finded.HaveUnreaded);
            
            return findedChat;
        }
        
        public async Task SetChatHaveUnreaded(int chatID, bool haveUnreaded)//отмечаем состояние новых сообщений в чате
        {
            try
            {
                var finded = await _connection.Table<ChatEntity>().FirstOrDefaultAsync(x => x.ID == chatID);
                if (finded != null)
                {
                    finded.HaveUnreaded = haveUnreaded;
                    await _connection.UpdateAsync(finded);
                }
            }
            catch (Exception e)
            {
                _logger.SaveMessage(LogEventType.Error, $"DBManager exception: {e.Message}");
            }
        }
        #endregion


        #region Messages
        public async Task<bool> SaveMessage(Message msg, bool isMessageFromServer)
        {
            try
            {
                var result = await _connection.InsertAsync(
                     new MessageItemEntity()
                     {
                         ID = msg.ID,
                         FromID = msg.FromID,
                         ChatID = msg.ChatID,
                         ToID = msg.ToID,
                         Type = msg.Type,
                         Data = msg.Data,
                         TimestampMilisec = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                         IsSended = isMessageFromServer//если оно с сервера -значит оно успешно доставлено и маркер выключаем
                     });

                if (isMessageFromServer && result == 1)//сразщу помечаем, что в чате появилось новое-непрочитанное
                    await SetChatHaveUnreaded(msg.ChatID, true);

                return result == 1;
            }
            catch (Exception e)
            {
                _logger.SaveMessage(LogEventType.Error, $"DBManager exception: {e.Message}");
                return false;
            }
        }
        public async Task<List<Message>> GetNonSended()
        {
            List<Message> result = new();
            var finded = await _connection.Table<MessageItemEntity>().Where(x => x.IsSended == false).ToListAsync();
            foreach (var item in finded)
                result.Add(
                    new Message(
                        item.ID,
                        item.FromID,
                        item.ChatID,
                        item.ToID,
                        item.Type,
                        item.Data,
                        new DateTime(item.TimestampMilisec).ToUniversalTime(),
                        item.IsSended));

            return result;
        }
        public async Task<bool> MarkMessageLikeSended(long msgID)
        {
            var finded = await _connection.Table<MessageItemEntity>().FirstOrDefaultAsync(x => x.ID== msgID);
            if (finded != null)
            {
                finded.IsSended = true;
                var updated = await _connection.UpdateAsync(finded);
                return updated == 1;
            }
            else
                return false;
        }

        public async Task<int> GetAllMessagesCount()
        {
            return await _connection.Table<MessageItemEntity>().CountAsync();
        }
        public async Task DeleteOld(int maxDaysHold)
        {
            if (maxDaysHold > 1)
            {
                var lastSaveDate = DateTimeOffset.UtcNow.AddDays(-1 * maxDaysHold).ToUnixTimeMilliseconds();
                await _connection.Table<MessageItemEntity>()
                    .Where(x => x.TimestampMilisec < lastSaveDate)
                    .DeleteAsync();
            }
        }

        public async Task<List<Message>> GetLastMessagesByChat(int chatID, int maxLenCount, DateTimeOffset startTimestamp)
        {
            List<Message> result = new();

            var t = startTimestamp.ToUnixTimeMilliseconds();
            var finded = await _connection.Table<MessageItemEntity>()
                .Where(x=>x.ChatID==chatID && x.TimestampMilisec< t)
                .OrderByDescending(x=>x.TimestampMilisec)
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
                        DateTimeOffset.FromUnixTimeMilliseconds(item.TimestampMilisec).UtcDateTime,
                        item.IsSended));

            await SetChatHaveUnreaded(chatID, false);//если запросили выборку - значит щас прочитают

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
