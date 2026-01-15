using Messaga_Client.Interfaces;
using Messaga_Client.Services.Repository;
using MessageLib;


namespace Messaga_Client.Services.DataProviders
{

    internal class MessageProvider(DBContext db) : IMessageProvider
    {
        private DBContext _dbContext = db;

        public void DeleteMessagesFromUser(string user)
        {
            object[] _params = { user };
            _dbContext.DB.ExecuteAsync("DELETE FROM Message WHERE FromUser=?", _params);
        }

        public void DeleteMessagesToUser(string user)
        {
            object[] _params = { user };
            _dbContext.DB.ExecuteAsync("DELETE FROM Message WHERE ToUser=?", _params);
        }

        public async Task<List<Message>> GetAllMessagesFromUser(string username)
        {
            List<Message> result = new();
            var finded = await _dbContext.DB.Table<MessageEntity>().Where(x => x.FromUser == username).ToListAsync();
            foreach (var msg in finded)
            {
                result.Add(new Message(
                    msg.ID,
                    msg.FromUser,
                    msg.ToUser,
                    msg.Type,
                    msg.MessageObject));
            }

            return result;
        }
        public async Task<List<Message>> GetAllMessagesToUser(string username)
        {
            List<Message> result = new();
            var finded = await _dbContext.DB.Table<MessageEntity>().Where(x => x.ToUser == username).ToListAsync();
            foreach (var msg in finded)
            {
                result.Add(new Message(
                    msg.ID,
                    msg.FromUser,
                    msg.ToUser,
                    msg.Type,
                    msg.MessageObject));
            }
            return result;
        }

        public async Task<bool> SaveMessage(Message message, bool isSended)
        {
            int saved = await _dbContext.DB.InsertAsync(
                new MessageEntity()
                {
                    IsSended = isSended,
                    SavedTime = DateTime.Now,
                    FromUser = message.FromUser,
                    ToUser = message.Reciever,
                    Type = message.Type,
                    MessageObject = MessageConvertor.Serialize(message.Data, message.Type)
                });

            return saved == 1;
        }


    }
}
