using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareP
{
    public class Group
    {
        public string name;
        public string hostName;
        public string hostIp;
        public byte[] password;
        public bool passwordProtected;
        public GroupSettings settings;
  
        public GroupNavigation navigation;

        public List<User> userList;


        public Group()
        {
            userList = new List<User>();
            settings = new GroupSettings();
        }

        public void AddUser(User user)
        {
            userList.Add(user);
            if (Connection.FormMenu != null)
            {
                Connection.FormMenu.FillHostUsersList();
                Connection.FormMenu.FillChatUsersList();
            }
        }

        public void RemoveUser(User user)
        {
            User toDelete = null;
            foreach (User u in userList)
            {
                if (u.Id == user.Id)
                {
                    toDelete = u;
                    break;
                }
            }
            if (toDelete != null)
                userList.Remove(toDelete);
            if (Connection.FormMenu != null)
            {
                Connection.FormMenu.FillHostUsersList();
                Connection.FormMenu.FillChatUsersList();
            }
        }

        public int GetUsersCount()
        {
            return userList.Count;
        }
    }

}
