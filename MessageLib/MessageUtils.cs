using System.Reflection.Metadata.Ecma335;
using System.Text;


namespace MessageLib
{
    public static class MessageUtils
    {
        public static byte[] TextToBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
        public static byte[] IntToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] BoolToBytes(bool val)
        {
            return BitConverter.GetBytes(val);
        }



        public static string GetDataLikeString(this Message msg)
        {
            if(msg.Data!=null && msg.Data.Length>0)
                return Encoding.UTF8.GetString(msg.Data);
            else
                return "";
        }
        public static int? GetDataLikeInt(this Message msg)
        {
            if (msg.Data != null && msg.Data.Length > 0)
                return BitConverter.ToInt32(msg.Data, 0);
            else
                return null;//чтобы понять что парсинг отвалился
        }
        public static bool? GetDataLikeBool(this Message msg)
        {
            if (msg.Data != null && msg.Data.Length > 0)
                return BitConverter.ToBoolean(msg.Data, 0);
            else
                return null;//чтобы понять что парсинг отвалился
        }

    }
}
