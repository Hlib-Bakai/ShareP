using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using ShareP.Server;

namespace ShareP.Controllers
{

    [ServiceContract(Namespace = "http://ShareP", CallbackContract = typeof(ISharePCallback),
                        SessionMode = SessionMode.Required)]
    public interface IShareP
    {
        //[OperationContract]
        //Dictionary<String, String> RequestServerInfo();

        //[OperationContract]
        //bool ClientConnect(Dictionary<String, String> clientInfo, byte[] password);

        [OperationContract(IsInitiating = true)]
        bool Connect(User user);

        [OperationContract(IsOneWay = true)]
        void Say(Message msg);

        [OperationContract(IsOneWay = true)]
        void IsWriting(User user);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect(User user);
        [OperationContract]
        Dictionary<string, string> RequestServerInfo();
    }

    public interface ISharePCallback //Presentation started, slide changed?, end presentation
    {
        [OperationContract(IsOneWay = true)]
        void RefreshUsers(List<User> users);  // Delete

        [OperationContract(IsOneWay = true)]
        void Receive(Message msg);

        [OperationContract(IsOneWay = true)]
        void IsWritingCallback(User user);
        
        [OperationContract(IsOneWay = true)]
        void UserJoin(User user);

        [OperationContract(IsOneWay = true)]
        void UserLeave(User user);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                        ConcurrencyMode = ConcurrencyMode.Multiple,
                        UseSynchronizationContext = false)]
    public class SharePService : IShareP
    {
        Dictionary<User, ISharePCallback> users =
         new Dictionary<User, ISharePCallback>();

        List<User> userList = new List<User>();

        public ISharePCallback CurrentCallback
        {
            get
            {
                return OperationContext.Current.
                       GetCallbackChannel<ISharePCallback>();
            }
        }

        object syncObj = new object();

        private bool SearchUsersByName(string name)
        {
            foreach (User u in users.Keys)
            {
                if (u.Username == name)
                {
                    return true;
                }
            }
            return false;
        }


        /// IShareP

        public bool Connect(User user)
        {
            if (!users.ContainsValue(CurrentCallback) && !SearchUsersByName(user.Username))
            {
                lock(syncObj)
                {
                    Connection.CurrentGroup.AddUser(user);
                    OnUserConnect(user);

                    foreach (User key in users.Keys)
                    {
                        ISharePCallback callback = users[key];
                        try
                        {
                            //callback.RefreshUsers(userList);
                            callback.UserJoin(user);
                        }
                        catch
                        {
                            users.Remove(key);
                        }
                    }
                    users.Add(user, CurrentCallback);
                    userList.Add(user);

                }

                Log.LogInfo("User connected: " + user.Username);
                return true;
            }
            Log.LogInfo("Refused connection from: " + user.Username); 
            return false;
        }
        public void Disconnect(User user)
        {
            Connection.CurrentGroup.RemoveUser(user);
            OnUserDisconnect(user);

            foreach (User u in users.Keys)
            {
                if (u.Username == user.Username)
                {
                    lock (syncObj)
                    {
                        this.users.Remove(u);
                        this.userList.Remove(u);
                        foreach (ISharePCallback callback in users.Values)
                        {
                            callback.RefreshUsers(this.userList);
                            callback.UserLeave(user);
                        }
                    }
                    return;
                }
            }
        }

        private void OnUserConnect(User user)
        {
            // Check config
            Notification.Show("User connected", user.Username + " joined");
        }

        private void OnUserDisconnect(User user)
        {
            // Check config
            Notification.Show("User disconnected", user.Username + " left");
        }

        public void Say(Message msg)
        {
            lock (syncObj)
            {
                foreach(ISharePCallback callback in users.Values)
                {
                    callback.Receive(msg);
                }
            }
        }

        public void IsWriting(User user)
        {
            lock (syncObj)
            {
                foreach (ISharePCallback callback in users.Values)
                {
                    callback.IsWritingCallback(user);
                }
            }
        }
        
        public Dictionary<string, string> RequestServerInfo()
        {
            var result = new Dictionary<string, string>();
            result.Add("GroupName", Connection.CurrentGroup.name);
            result.Add("HostName", Connection.CurrentGroup.hostName);
            result.Add("NumberOfUsers", "20");  // TODO
            result.Add("Password", Connection.CurrentGroup.passwordProtected.ToString());
            result.Add("Download", Connection.CurrentGroup.settings.Download.ToString());
            result.Add("ViewersPresent", Connection.CurrentGroup.settings.Viewerspresent.ToString());
            return result;
        }
    }


    static class ServerController
    {
        public static Group MyGroup
        {
            get;
            set;
        }

        static ServiceHost SelfHost;

        public static void StartServer()
        {
            string ipBase = Helper.GetMyIP();

            Uri tcpAdrs = new Uri("net.tcp://" + ipBase + ":8000/ShareP/");
            Uri httpAdrs = new Uri("http://" + ipBase + ":8001/ShareP/");

            Uri[] baseAdresses = { tcpAdrs, httpAdrs };

            SelfHost = new ServiceHost(
                            typeof(SharePService), baseAdresses);

            NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.None, true);
            tcpBinding.MaxBufferPoolSize = (int)67108864;
            tcpBinding.MaxBufferSize = 67108864;
            tcpBinding.MaxReceivedMessageSize = (int)67108864;
            tcpBinding.TransferMode = TransferMode.Buffered;
            tcpBinding.ReaderQuotas.MaxArrayLength = 67108864;
            tcpBinding.ReaderQuotas.MaxBytesPerRead = 67108864;
            tcpBinding.ReaderQuotas.MaxStringContentLength = 67108864;

            tcpBinding.MaxConnections = 100; // Maybe change?

            ServiceThrottlingBehavior throttle; //Производительность
            throttle = SelfHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (throttle == null)
            {
                throttle = new ServiceThrottlingBehavior();
                throttle.MaxConcurrentCalls = 100;              //Here as well
                throttle.MaxConcurrentSessions = 100;           //--
                SelfHost.Description.Behaviors.Add(throttle);
            }

            tcpBinding.ReceiveTimeout = new TimeSpan(24, 0, 0); // Keep sessions alive for 24 hours
            tcpBinding.ReliableSession.Enabled = true;
            tcpBinding.ReliableSession.InactivityTimeout = new TimeSpan(24, 0, 0);

            SelfHost.AddServiceEndpoint(typeof(IShareP), tcpBinding, "tcp");

            ServiceMetadataBehavior metadataBehavior = new ServiceMetadataBehavior();
            SelfHost.Description.Behaviors.Add(metadataBehavior);

            SelfHost.AddServiceEndpoint(typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexTcpBinding(),
                "net.tcp://" + ipBase +
                ":8002/ShareP/mex");

            
            try
            {
                SelfHost.Open();
            }
            catch (Exception e)
            {
                //
                Log.LogException(e);
            }
            finally
            {
                if (SelfHost.State == CommunicationState.Opened)
                {
                    Log.LogInfo("Server opened on " + ipBase);
                }
            }
            
        }
        

        public static void StopServer()
        {
            if (SelfHost != null)
            {
                try
                {
                    SelfHost.Close();
                }
                catch (Exception e)
                {
                    Log.LogException(e, "Can't stop server.");
                }
                finally
                {
                    if (SelfHost.State == CommunicationState.Closed)
                    {
                        Log.LogInfo("Server closed");
                    }
                }
            }
        }
    }
}
