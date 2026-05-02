using Penta_Server.Interfaces;
using MessageLib;
using Microsoft.EntityFrameworkCore;
using Penta_Server.Services.Repositories.Models;


namespace Penta_Server.Services.Repositories
{
    public class MessageRepository(IConfiguration config): IMessageRepository
    {
       private string connStr = config["WorkDB:ConnString"];


        public void DeleteMessage(Guid messageID)
        {
            using (DB db = new DB(connStr))
            {
                var finded = db.Messages.FirstOrDefault(x => x.ID == messageID);
                if(finded!=null)
                {
                    var data =db.SharedDatas.FirstOrDefault(x => x.ID == finded.SharedDataID && finded.SharedDataID!=null);
                    if(data!=null)
                    {
                        data.CopyCount--;
                        if(data.CopyCount<=0)
                            db.SharedDatas.Remove(data);//все необходимые копии использованы, можно удалять
                    }

                    db.Messages.Remove(finded);

                    db.SaveChanges();
                }
            }
        }


        

        public void SaveWithData(Message msg,Guid sharedMarker, int dataCopyCount)
        {
            using (DB db= new DB(connStr))
            {
                if (db.Users.FirstOrDefault(x => x.ID == msg.ToID) != null)//получатель должен быть зарегистрированнным
                {
                    Guid createdSharedDataID;
                    var findedCopy = db.SharedDatas.FirstOrDefault(x => x.SharedMarker == sharedMarker);
                    if (findedCopy == null)
                    {
                        var dataEntity = new SharedDataEntity()
                        {
                            ID = Guid.NewGuid(),
                            SharedMarker = sharedMarker,
                            Data = msg.Data,
                            CopyCount = dataCopyCount,
                        };
                        db.SharedDatas.Add(dataEntity);
                        createdSharedDataID = dataEntity.ID;
                    }
                    else//кто-то уже создал данные с указанным маркером
                        createdSharedDataID = findedCopy.ID;

                    var createdEntity = new MessageEntity()
                    {
                        ID = msg.ID,
                        IsSended = false,
                        FromUserID = msg.FromID,
                        GroupID = msg.ChatID,
                        ToUserID = msg.ToID,
                        Type = msg.Type,
                        SharedDataID = createdSharedDataID,//указываем к каким данным привязываемся
                        UtcTimestamp = msg.UtcTimestamp,
                    };
                    db.Messages.Add(createdEntity);
                    db.SaveChanges();
                }
            }
        }

        public void SaveWithoutData(Message msg)
        {
            using (DB db = new DB(connStr))
            {
                if (db.Users.FirstOrDefault(x => x.ID == msg.ToID) != null)//получатель должен быть зарегистрированнным
                {
                    var createdEntity = new MessageEntity()
                    {
                        ID = msg.ID,
                        IsSended = false,
                        FromUserID = msg.FromID,
                        GroupID = msg.ChatID,
                        ToUserID = msg.ToID,
                        Type = msg.Type,
                        SharedDataID = null,
                        UtcTimestamp = msg.UtcTimestamp,
                    };
                    db.Messages.Add(createdEntity);
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
                        byte[] data = null;

                        foreach (var f in finded)
                        {
                            if (f.SharedDataID !=null) //значит какие-то данные есть, надо искать
                                data = db.SharedDatas.FirstOrDefault(x => x.ID == f.SharedDataID).Data;
                            else
                                data = null;

                            result.Add(new Message(
                                f.ID,
                                f.FromUserID,
                                f.GroupID,
                                f.ToUserID,
                                f.Type,
                                data,
                                f.UtcTimestamp,
                                false));
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
                return finded != null && finded.Count > 0;
            }
        }


    }
}
