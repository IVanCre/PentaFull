using Penta_Server.Interfaces;
using MessageLib;
using Microsoft.EntityFrameworkCore;
using Penta_Server.Services.Repositories.Models;


namespace Penta_Server.Services.Repositories
{
    public class MessageRepository(IConfiguration config): IMessageRepository
    {
       private string connStr = config["WorkDB:ConnString"];


        public void DeleteMessage(long messageID)
        {
            using (DB db = new DB(connStr))
            {
                var finded = db.Messages.FirstOrDefault(x => x.ID == messageID);
                if(finded!=null)
                {
                    db.Messages.Remove(finded);
                    var data =db.SharedDatas.FirstOrDefault(x => x.ID == finded.SharedDataID);
                    if(data!=null)
                    {
                        data.CopyCount--;
                        if(data.CopyCount<=0)
                            db.SharedDatas.Remove(data);//все необходимые копии использованы, можно удалять
                    }
                    db.SaveChanges();
                }
            }
        }

        public long SaveDataLikeShared(long sharedMarker, byte[] data, int copyCount)
        {
            using (DB db = new DB(connStr))
            {
                var findedCopy =db.SharedDatas.FirstOrDefault(x => x.SharedMarker == sharedMarker);
                if (findedCopy == null)
                {
                    var dataEntity = new SharedDataEntity()
                    {
                        SharedMarker = sharedMarker,
                        Data = data,
                        CopyCount = copyCount,
                    };
                    db.SharedDatas.Add(dataEntity);

                    db.SaveChanges();
                    return dataEntity.ID;
                }
                else//кто-то уже создал данные, привязываемся к этому объекту
                    return findedCopy.ID;
            }
        }
        public void SaveWithData(Message msg,long sharedDataID)
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
                        SharedDataID = sharedDataID,
                        UtcTimestamp = msg.UtcTimestamp,
                    };
                    db.Messages.Add(createdEntity);
                    db.SaveChanges();

                    msg.SetServerID(createdEntity.ID);//меняем на серверный идентификатор(чтобы локальные айцдишникис  клиентов не конфликтовали на сервере)
                }
            }
        }
        public void SaveWithoutData(Message msg) => SaveWithData(msg, -1);



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
                            if (f.SharedDataID > -1) //значит какие-то данные есть, надо искать
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
