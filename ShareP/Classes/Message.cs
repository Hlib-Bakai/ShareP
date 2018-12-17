using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShareP.Server
{
    [DataContract]
    public class Message
    {
        private string sender;
        private string senderIp;
        private string text;
        private DateTime time;

        [DataMember]
        public string Sender
        {
            get
            {
                return sender;
            }
            set
            {
                sender = value;
            }
        }

        [DataMember]
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        [DataMember]
        public DateTime Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
            }
        }

        [DataMember]
        public string SenderIp
        {
            get
            {
                return senderIp;
            }
            set
            {
                senderIp = value;
            }
        }
    }
}
