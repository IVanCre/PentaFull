using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Penta_Server.Services.Repositories.Models
{
    [Index("ID", IsUnique = true)]
    [Index("Name")]
    public class GroupEntity
    {
        public int ID { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public int AdminGroupID { get; set; }// id юзера, который управляет группой

        public string UsersInGroup { get; set; }// формат: "11,23,74,31...."
    }


    public static class GroupEntityExtension
    { 
        public static void AddToGroup(this GroupEntity group, int userID)
        {
            if (string.IsNullOrEmpty(group.UsersInGroup))
                group.UsersInGroup = $"{userID}";
            else
                group.UsersInGroup = group.UsersInGroup + $",{userID}";
        }

        public static void RemoveFromGroup(this GroupEntity group, int userID)
        {
            StringBuilder str = new StringBuilder();
            string[] usersID = group.UsersInGroup.Split(new char[] { ',' });
            var userIDStr = userID.ToString();
            foreach (var id in usersID)
            {
                if (id != userIDStr)
                {
                    if (str.Length == 0)
                        str.Append(",");
                    else
                        str.Append(",");
                }
            }

            group.UsersInGroup= str.ToString();
        }
        public static int[] UserIDsInGroup(this GroupEntity group)
        {
            string[] usersID = group.UsersInGroup.Split(new char[] { ',' });
            int[] idArr= new int[usersID.Length];
            for (int i = 0;i<usersID.Length;i++)
                idArr[i]= int.Parse(usersID[i]);

            return idArr;
        }
    }
}
