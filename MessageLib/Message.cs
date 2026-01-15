
using System.Text.Json.Serialization;


namespace MessageLib
{
    public enum MessageType
    {
        Unknown = 0,

#region FromUserToUser

        Text = 10,//текстовое сообщение от юзера к юзеру
        Picture = 11,//изображение от юзера к юзеру
        Voice = 12,//голосовое от юзера к юзеру
        EnterToGroupRequest = 13,//запрос на вступление в группу (user->user)
#endregion

#region FromUserToSystem
        
        EnterToGroupResponce = 50,//ответ на приглашение в группу (user->system)
        LeaveGroupRequest = 51,//юзер сам выходит (user->system)
        RemoveUserFromGroupRequest = 52,//юзера выкидывает сам админ группы (user->system)
        CreateGroupRequest = 53,//создание новой группы (user->ыныеуь)
        DeleteGroupRequest = 54,//удаление группы и ее чатов самим админом группы (user->system)
#endregion
    }

    public class Message
    {
        public int ID { get; private set; }
        public int FromUserID { get; private set; }
        public int GroupID { get; private set; }
        public int ToUserID { get; private set; }
        public MessageType Type { get; private set; }
        public byte[] Data { get; private set; }

        [JsonConstructor]
        public Message(
           int ID,
           int FromUserID,
           int GroupID,
           int ToUserID,
           MessageType Type,
           byte[] Data)
        {
            this.ID = ID;
            this.FromUserID = FromUserID;
            this.GroupID = GroupID;
            this.ToUserID = ToUserID;
            this.Type = Type;
            this.Data = Data;
        }
    }
}
