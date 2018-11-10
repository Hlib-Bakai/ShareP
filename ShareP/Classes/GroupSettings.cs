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
            Chat = false;
            NConnected = false;
            NDisconnected = false;
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
        public bool Chat
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
    }
}
