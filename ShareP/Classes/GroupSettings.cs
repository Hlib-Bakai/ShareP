using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareP
{
    public class GroupSettings
    {
        public GroupSettings()
        {
            Download = false;
            Viewerspresent = false;
            NConnected = false;
            NDisconnected = false;
            NChat = false;
            NCheater = false;
        }

        public bool Download
        {
            get;
            set;
        }
        public bool Viewerspresent
        {
            get;
            set;
        }
        public bool NConnected
        {
            get;
            set;
        }
        public bool NDisconnected
        {
            get;
            set;
        }
        public bool NChat
        {
            get;
            set;
        }
        public bool NCheater
        {
            get;
            set;
        }
    }
}
