

using System.Text;


namespace Penta_ClientLib.Services
{
    public static class ContactConverter
    {
        private const uint _mask = 3_305_078_396;

        public static int ExtractUserID(string userContactID)
        {
            try
            {
                StringBuilder str = new(userContactID);
                str.Replace("-", "");//вырезаем тире
                str.Remove(0, 1);//вырезаем бесполезную восьмерку

                var masked = long.Parse(str.ToString());
                return Convert.ToInt32(_mask - masked);
            }
            catch(Exception e)
            {
                return -1;
            }
        }
        public static string ConvertUserIDToContactID(int userID)
        {            
            var masked = (_mask - userID).ToString();
            StringBuilder str = new(masked);
            str.Insert(0, "8");
            str.Insert(1, '-');
            str.Insert(5, '-');
            str.Insert(9, '-');
            str.Insert(12, '-');// формат типа телефона 8-330-507-83-96 ))))))

            return str.ToString();
        }
    }
}
