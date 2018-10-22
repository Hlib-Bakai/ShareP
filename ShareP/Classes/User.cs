using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShareP
{
    [DataContract]
    public class User
    {
        string username;
        string ip;
        int id;

        public User()
        {
            if (!String.IsNullOrEmpty(Properties.Settings.Default["username"].ToString()))
            {
                username = Properties.Settings.Default["username"].ToString();
            }
            else
            {
                try
                {
                    ChangeUsername(System.Environment.MachineName);
                }
                catch (Exception e)
                {
                    Log.LogException(e, "Can't get computer's name. Using default one");
                }
                if (String.IsNullOrEmpty(username))
                    ChangeUsername("User #" + (new Random(Guid.NewGuid().GetHashCode())).Next(1, 10000).ToString());
            }

            this.ip = Helper.GetMyIP();
        }

        public void ChangeUsername(string newUsername)
        {
            username = newUsername;
            Properties.Settings.Default["username"] = newUsername;
            Properties.Settings.Default.Save();
            Log.LogInfo(String.Format("Username changed to {0}.", newUsername));
        }

        [DataMember]
        public string Username
        {
            get
            {
                return username;
            }
        }

        [DataMember]
        public string IP
        {
            get
            {
                return ip;
            }
            set
            {
                ip = value;
            }
        }

        [DataMember]
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
    }
}
