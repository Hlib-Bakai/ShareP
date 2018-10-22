using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShareP.Classes
{
    [DataContract]
    class Message
    {
        private string sender;
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
    }
}
