using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShareP.Server
{
    [DataContract]
    public class User
    {
        string username;
        string ip;
        int id;
        

        

        [DataMember]
        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
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
