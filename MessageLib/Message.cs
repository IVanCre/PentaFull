
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
        LeaveGroupResponce=52,

        RemoveUserFromGroupRequest = 53,//юзера выкидывает сам админ группы (user->system)
        RemoveUserFromGroupResponce=54,

        CreateGroupRequest = 55,//создание новой группы (user->ыныеуь)
        CreateGroupResponce=56,

        DeleteGroupRequest = 57,//удаление группы и ее чатов самим админом группы (user->system)
        DeleteGroupResponce=58
#endregion
    }

    public class Message
    {
        public int ID { get; private set; }
        public int FromID { get; private set; }
        public int ChatID { get; private set; }
        public int ToID { get; private set; }
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
            this.FromID = FromUserID;
            this.ChatID = GroupID;
            this.ToID = ToUserID;
            this.Type = Type;
            this.Data = Data;
        }
    }
}
