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
        private int nextId;


        public Group()
        {
            userList = new List<User>();
            nextId = 0;
            settings = new GroupSettings();
        }

        public void AddUser(User user)
        {
            user.Id = nextId;
            nextId++;
            userList.Add(user);
            Connection.FormMenu.FillHostUsersList();
        }

        public void RemoveUser(User user)
        {
            User toDelete = null;
            foreach (User u in userList)
            {
                if (u.Username == user.Username)
                {
                    toDelete = u;
                    break;
                }
            }
            if (toDelete != null)
                userList.Remove(toDelete);
            Connection.FormMenu.FillHostUsersList();
        }

        public int GetUsersCount()
        {
            return userList.Count;
        }
    }

    public enum GroupNavigation
    {
        FollowOnly,
        Backwards,
        BothDirections
    }
}
