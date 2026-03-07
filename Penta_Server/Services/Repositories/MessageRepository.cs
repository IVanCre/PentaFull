using Penta_Server.Interfaces;
using MessageLib;
using Microsoft.EntityFrameworkCore;
using Penta_Server.Services.Repositories.Models;


namespace Penta_Server.Services.Repositories
{
    public class MessageRepository(IConfiguration config): IMessageRepository
    {
       private string connStr = config["WorkDB:ConnString"];


        public async void MarkForDelete(int userID, long messageID)
        {
            using (DB db = new DB(connStr))
            {
                await db.Database.ExecuteSqlRawAsync($"UPDATE Messages SET IsSended=1 where ID={messageID}");
            }
        }

        public void Add(Message msg)
        {
            using (DB db= new DB(connStr))
            {
                if(db.Users.FirstOrDefault(x=>x.ID==msg.ToID) !=null)//получатель должен быть зарегистрированнным
                {
                    var createdEntity = new MessageEntity()
                    {
                        //оригинальный message.id(сгенеренный клиентом) не используем- т.к. клиенты шлют отрицательные идентификаторы(и они могут повторяться)
                        IsSended = false,
                        FromUserID = msg.FromID,
                        GroupID = msg.ChatID,
                        ToUserID = msg.ToID,
                        Type = msg.Type,
                        Data = msg.Data,
                        UtcTimestamp = msg.UtcTimestamp,
                    };
                    db.Messages.Add(createdEntity);
                    db.SaveChanges();

                    msg.SetNewID(createdEntity.ID);//меняем на серверный идентификатор
                }
            }
        }

        public async Task<List<Message>> GetNonSendedForUserAsync(int userID)
        {
            return await Task.Factory.StartNew(() =>
            {
                var result = new List<Message>();
                using (DB db = new DB(connStr))
                {
                    if (db.Users.FirstOrDefault(x => x.ID == userID) != null)//получатель должен быть зарегистрированнным
                    {
                        var finded = db.Messages.Where(x => x.ToUserID==userID && !x.IsSended).ToList();
                        foreach (var f in finded)
                        {
                            result.Add(new Message(
                                f.ID,
                                f.FromUserID,
                                f.GroupID,
                                f.ToUserID,
                                f.Type,
                                f.Data,
                                f.UtcTimestamp));
                        }
                    }
                }
                return result;
            });
        }

        public async Task<bool> HasNonSended(int userID)
        {
            using (DB db = new DB(connStr))
            {
                var finded = await db.Messages.Where(x => x.ToUserID == userID && !x.IsSended).ToListAsync();
                db.SaveChanges();

                return finded != null && finded.Count > 0;
            }
        }
    }
}
