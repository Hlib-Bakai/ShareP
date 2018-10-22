using ShareP.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ShareP
{
    static class Connection
    {
        private static Group currentGroup;
        private static SharePClient client;
        private static Role role = Role.Notconnected;
        private static User currentUser;

        
        public static Group CurrentGroup
        {
            get
            {
                return currentGroup;
            }
            set
            {
                currentGroup = value;
            }
        }

        public static User CurrentUser
        {
            get
            {
                return currentUser;
            }
            set
            {
                currentUser = value;
            }
        }

        public static void SetRole(Role newRole)
        {
            role = newRole;
        }

        public static Role GetRole()
        {
            return role;
        }

        public static ConnectionResult EstablishClientConnection(string ip, byte[] password = null)
        {
            try
            {
                client = new SharePClient(new BasicHttpBinding(), new EndpointAddress("http://" + ip + ":8000/ShareP/Service/SharePService"));
                var clientInfo = new Dictionary<string, string>();
                clientInfo.Add("username", currentUser.Username);
                clientInfo.Add("IP", Helper.GetMyIP());
                if (client.ClientConnect(clientInfo, password))
                {
                    LoadGroupParameters(ip);
                    role = Role.Client;
                }
                else
                {
                    return ConnectionResult.WrongPassword;
                }
            }
            catch (Exception e)
            {
                Log.LogException(e, "Failed to establish connection! ");
                return ConnectionResult.Error;
            }
            return ConnectionResult.Success;
        }
        
        private static void LoadGroupParameters(string ip)
        {
            Group group = new Group();
            group.hostIp = ip;

            var groupInfo = client.RequestServerInfo();
            group.name = groupInfo["GroupName"];
            group.hostName = groupInfo["HostName"];

            CurrentGroup = group;
        }

        public static void CreateGroup(Group group)
        {
            ServerController.StartServer();
            CurrentGroup = group;
            SetRole(Role.Host);
        }
        
        public static void Disconnect()
        {
            if (role == Role.Client)
                DisconnectClient();
            else if (role == Role.Host)
                DisconnectServer();
        }

        private static void DisconnectClient()
        {
            //Send a server command about disconnect
            client.Close();
            CurrentGroup = null;
            role = Role.Notconnected;
        }

        private static void DisconnectServer()
        {
            //Send client commands to disconnect
            ServerController.StopServer();
            CurrentGroup = null;
            role = Role.Notconnected;
        }

        public enum Role
        {
            Host,
            Client,
            Notconnected
        }

        public enum ConnectionResult
        {
            Success, 
            WrongPassword,
            Error
        }
        
    }
}
