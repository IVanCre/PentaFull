using System.Text;


namespace MessageLib
{
    public static class MessageUtils
    {
        public static byte[] TextToBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
        public static byte[] BooleanToBytes(bool val)
        {
            return BitConverter.GetBytes(val);
        }
        public static byte[] IntToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }


        public static string GetDataLikeString(this Message msg)
        {
            return Encoding.UTF8.GetString(msg.Data);
        }
        public static bool GetDataLikeBoolean(this Message msg)
        {
            return (msg.Data[0] == 1);
        }
        public static int GetDataLikeInt(this Message msg)
        {
           return BitConverter.ToInt32(msg.Data, 0);
        }


        /// <summary>
        /// Определение идет по типу MessageType
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool IsUserToUser(this Message msg)
        {
            return msg.Type > MessageType.Unknown && msg.Type < MessageType.EnterToGroupResponce;
        }

    }
}
