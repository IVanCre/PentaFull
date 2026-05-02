using Microsoft.EntityFrameworkCore;

namespace Penta_Server.Services.Repositories.Models
{
    [Index("SharedMarker")]
    public class SharedDataEntity
    {
        public Guid ID { get; set; }
//маркер нужен, чтобы система могла опознать, когда создавать объект, а когда надо использовать
//уже созданный
        public Guid SharedMarker { get; set; }
        public byte[] Data { get; set; }

//используется, когда копия этих данных отправлется с очередным ответом члену группы.
//как только счетчик дойдет до 0, значит все копии использованы, и этот объект более не нужен
        public int CopyCount { get; set; }
    }
}
