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
        public bool download;
        public bool viewerspresent;
        public bool nConnected;
        public bool nDisconnected;
        public bool nChat;
        public bool nCheater;
        public GroupNavigation navigation;
        public FormMenu formMenu;

        public List<User> userList;
        private int nextId;
        

        public Group()
        {
            userList = new List<User>();
            nextId = 0;
        }

        public void AddUser(User user)
        {
            user.Id = nextId;
            nextId++;
            userList.Add(user);
            formMenu.FillHostUsersList();
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
