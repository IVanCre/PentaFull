namespace Penta_Server.Services.Repositories.Models
{
    public class SharedDataEntity
    {
        public long ID { get; set; }
        public byte[] Data { get; set; }

//используется, когда копия этих данных отправлется с очередным ответом члену группы.
//как только счетчик дойдет до 0, значит все копии использованы, и этот объект более не нужен
        public int CopyCount { get; set; }
    }
}
