using Message_Server.Interfaces;
using MessageLib;
using Microsoft.EntityFrameworkCore;

namespace Message_Server.Services.Repositories
{
    public class MessageRepository(IConfiguration config): IMessageRepository
    {
       private string connStr = config["WorkDB:ConnString"];


        public void MarkForDelete(int messageID)
        {
            using (DB db = new DB(connStr))
            {
                db.Database.ExecuteSqlRaw($"UPDATE Messages SET IsSended=1 where ID={messageID}");
            }
        }

        public void Add(Message msg)
        {
            using (DB db= new DB(connStr))
            {
                if(db.Users.FirstOrDefault(x=>x.ID==msg.ToID) !=null)//получатель должен быть зарегистрированнным
                {
                    db.Messages.Add(new Models.MessageEntity()
                    {
                        IsSended = false,
                        FromUserID = msg.FromID,
                        GroupID = msg.GroupID,
                        ToUserID = msg.ToID,
                        Type = msg.Type,
                        Data = msg.Data
                    });
                    db.SaveChanges();
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
                                f.Data));
                        }
                    }
                }
                return result;
            });
        }

        public bool HasNonSended(int userID)
        {
            using (DB db = new DB(connStr))
            {
                var finded = db.Messages.FirstOrDefault(x => x.ToUserID==userID&& !x.IsSended);
                return finded != null;   
            }
        }
    }
}
