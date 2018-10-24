using ShareP.Controllers;
using ShareP.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using static ShareP.Connection;

namespace ShareP
{
    public class ClientConnection : ISharePCallback
    {
        SharePClient client = null;
        string rcvFilesPath = @"TODO";
        private delegate void FaultedInvoker();
        List<User> onlineUsers = new List<User>();


        void HandleConnection()
        {
            // Do something if connection lost or created
        }


        void InnerDuplexChannel_Closed(object sender, EventArgs e)
        {
            Log.LogInfo("Channel closed");
            HandleConnection();
        }

        void InnerDuplexChannel_Opened(object sender, EventArgs e)
        {
            Log.LogInfo("Channel opened");
            HandleConnection();
        }

        void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        {
            Log.LogInfo("Channel faulted");
            HandleConnection();
        }

        public Dictionary<string, string> GetServiceOnIP(string ip)
        {
            try
            {
                InstanceContext instanceContext = new InstanceContext(this);
                var temp = new SharePClient(instanceContext);
                string servicePath = temp.Endpoint.ListenUri.AbsolutePath;


                temp.Endpoint.Address = new EndpointAddress("net.tcp://" + ip + ":8000" + servicePath);  

                temp.Open();

                var serviceData = temp.RequestServerInfo();

                temp.Close();

                return serviceData;
            }
            catch
            {
                return null;
            }
        }


        public ConnectionResult EstablishClientConnection(string ip, byte[] password = null)
        {
            if (client == null)
            {
                try
                {
                    InstanceContext instanceContext = new InstanceContext(this);
                    client = new SharePClient(instanceContext);
                    string servicePath = client.Endpoint.ListenUri.AbsolutePath;


                    client.Endpoint.Address = new EndpointAddress("net.tcp://" + ip + ":8000" + servicePath);  // need ":"?

                    client.Open();

                    Log.LogInfo("Connection opened");

                    client.InnerDuplexChannel.Faulted +=
                      new EventHandler(InnerDuplexChannel_Faulted);
                    client.InnerDuplexChannel.Opened +=
                      new EventHandler(InnerDuplexChannel_Opened);
                    client.InnerDuplexChannel.Closed +=
                      new EventHandler(InnerDuplexChannel_Closed);

                    if (client.Connect(Connection.CurrentUser))
                    {
                        return ConnectionResult.Success;
                    }
                    else
                        return ConnectionResult.WrongPassword;
                }
                catch (Exception e)
                {
                    client = null;
                    Log.LogException(e, "Error during connection.");
                    return ConnectionResult.Error;
                }
            }
            return ConnectionResult.Error;
        }

        public Dictionary<string, string> RequestServerInfo()
        {
            return client.RequestServerInfo();
        }


        public void Disconnect()
        {
            if (client == null)
                return;

            client.DisconnectAsync(Connection.CurrentUser);

        }


        public void IsWritingCallback(User user)
        {
            if (user != null)
            {
                /// User is writing
            }
        }

        public void Receive(Message msg)
        {
            // We got message in chat
        }

        public void RefreshUsers(User[] users)
        {
            onlineUsers = new List<User>(users);

            // Refresh list of users
        }

        public void UserJoin(User user)
        {
            FormAlert formAlert = new FormAlert("New user", "Meet new user: " + user.Username, true);
            formAlert.ShowDialog();

            // NOTIFICATION !
        }

        public void UserLeave(User user)
        {
            FormAlert formAlert = new FormAlert("User left", "Disconnected user: " + user.Username, true);
            formAlert.ShowDialog();
        }
    }

    static public class Connection
    {
        static private Group currentGroup;
        //private static SharePClient client;
        static private Role role = Role.Notconnected;
        static private User currentUser = null;
        
        static string rcvFilesPath = @"TODO";
        private delegate void FaultedInvoker();
        static List<User> onlineUsers = new List<User>();
        static ClientConnection clientConnection = new ClientConnection();


        public static ConnectionResult EstablishClientConnection(string ip, byte[] password = null)
        {
            var result = clientConnection.EstablishClientConnection(ip, password);
            if (result == ConnectionResult.Success)
            {
                LoadGroupParameters(ip);
                CurrentRole = Role.Client;
            }
            return result;
        }

        private static void LoadGroupParameters(string ip)
        {
            Group group = new Group();
            group.hostIp = ip;

            var groupInfo = clientConnection.RequestServerInfo();
            group.name = groupInfo["GroupName"];
            group.hostName = groupInfo["HostName"];

            CurrentGroup = group;
        }

        public static Dictionary<string, string> GetServiceOnIP(string ip)
        {
            return clientConnection.GetServiceOnIP(ip);
        }

        public static void CreateGroup(Group group)
        {
            ServerController.StartServer();
            CurrentGroup = group;
            CurrentRole = Role.Host;
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
            //client.Close();
            clientConnection.Disconnect();
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


        public static Role CurrentRole
        {
            get
            {
                return role;
            }
            set
            {
                role = value;
            }
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
