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
using System.IO;

namespace ShareP.Controllers
{

    [ServiceContract(Namespace = "http://ShareP", CallbackContract = typeof(ISharePCallback),
                        SessionMode = SessionMode.Required)]
    public interface IShareP                                        // METHODS FOR CLIENTS, SHOULD BE HANDLED IN SERVER
    {
        [OperationContract(IsInitiating = true)]
        ConnectionResult Connect(User user);

        [OperationContract(IsOneWay = true)]
        void Say(Message msg);

        [OperationContract(IsOneWay = true)]
        void IsWriting(User user);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect(User user);

        [OperationContract]
        Dictionary<string, string> RequestServerInfo();

        [OperationContract]
        byte[] RequestSlide(int slide);

        [OperationContract]
        Presentation RequestCurrentPresentation();
        
        [OperationContract(IsOneWay = true)]
        void ViewerChangeFocus(bool focus, User user);


        // Client presentations
  
        [OperationContract(IsOneWay = true)]
        void ClPresentationStarted(Presentation presentation, User user);

        [OperationContract(IsOneWay = true)]
        void ClPresentationNextSlide(int slide);

        [OperationContract(IsOneWay = true)]
        void ClPresentationEnd();
    }

    public interface ISharePCallback                                 // METHODS FOR SERVER TO SEND DATA TO CLIENTS. CLIENTS SHOULD HANDLE
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

        [OperationContract(IsOneWay = true)]
        void KickUser();

        [OperationContract(IsOneWay = true)]
        void GroupSettingsChanged(Dictionary<string, string> newSettings);

        [OperationContract(IsOneWay = true)]
        void PresentationStarted(Presentation presentation);

        [OperationContract(IsOneWay = true)]
        void PresentationNextSlide(int slide);

        [OperationContract(IsOneWay = true)]
        void PresentationEnd();

        [OperationContract(IsOneWay = true)]
        void GroupClose();

        [OperationContract]
        byte[] ClRequestSlide(int slide);
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
                if (u.Username.CompareTo(name) == 0)
                {
                    return true;
                }
            }
            if (Connection.CurrentUser.Username.CompareTo(name) == 0)
                return true;
            return false;
        }

        public void OnPresentationStart(Presentation presentation)
        {
            lock (syncObj)
            {
                foreach (User key in users.Keys)
                {
                    ISharePCallback callback = users[key];
                    if (key.Username.CompareTo(presentation.Author) == 0)
                        continue;
                    try
                    {
                        callback.PresentationStarted(presentation);
                    }
                    catch (Exception ex)
                    {
                        Log.LogException(ex, "OnPresentationStart Service");
                    }
                }
            }
        }

        public void OnGroupClose()
        {
            lock (syncObj)
            {
                foreach (User key in users.Keys)
                {
                    ISharePCallback callback = users[key];
                    try
                    {
                        callback.GroupClose();
                    }
                    catch (Exception ex)
                    {
                        Log.LogException(ex, "OnGroupClose Service");
                    }
                }
            }
        }

        public void OnPresentationNextSlide(int slide)
        {
            lock (syncObj)
            {
                foreach (User key in users.Keys)
                {
                    ISharePCallback callback = users[key];
                    try
                    {
                        callback.PresentationNextSlide(slide);
                    }
                    catch (Exception ex)
                    {
                        Log.LogException(ex, "OnPresentationNextSlide Service");
                    }
                }
            }
        }

        public void OnPresentationEnd()
        {
            lock (syncObj)
            {
                foreach (User key in users.Keys)
                {
                    ISharePCallback callback = users[key];
                    try
                    {
                        callback.PresentationEnd();
                    }
                    catch (Exception ex)
                    {
                        Log.LogException(ex, "OnPresentationEnd Service");
                    }
                }
            }
        }

        public void OnGroupSettingsChanged()
        {
            lock (syncObj)
            {
                foreach (User key in users.Keys)
                {
                    ISharePCallback callback = users[key];
                    try
                    {
                        var result = new Dictionary<string, string>();
                        result.Add("Download", Connection.CurrentGroup.settings.Download.ToString());
                        result.Add("ViewersPresent", Connection.CurrentGroup.settings.Viewerspresent.ToString());
                        if (Connection.CurrentGroup.navigation == GroupNavigation.Backwards)
                            result.Add("GroupNavigation", "Backwards");
                        else if (Connection.CurrentGroup.navigation == GroupNavigation.BothDirections)
                            result.Add("GroupNavigation", "Both");
                        else if (Connection.CurrentGroup.navigation == GroupNavigation.FollowOnly)
                            result.Add("GroupNavigation", "Follow");

                        callback.GroupSettingsChanged(result);
                    }
                    catch (Exception ex)
                    {
                        Log.LogException(ex, "OnGroupSettingsChanged Service");
                    }
                }
            }
        }


        /// IShareP

        public ConnectionResult Connect(User user)
        {
            if (!users.ContainsValue(CurrentCallback))
            {
                lock (syncObj)
                {
                    if (SearchUsersByName(user.Username))
                        return ConnectionResult.UsernameExists;
                    Connection.CurrentGroup.AddUser(user);
                    OnUserConnect(user);
                    PresentationController.UserConnected(user);

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
                return ConnectionResult.Success;
            }
            Log.LogInfo("Refused connection from: " + user.Username);
            return ConnectionResult.Error;
        }
        public void Disconnect(User user)
        {
            Connection.CurrentGroup.RemoveUser(user);
            OnUserDisconnect(user);
            PresentationController.UserDisconneced(user);

            if (Connection.CurrentPresentation != null && Connection.CurrentPresentation.Author.CompareTo(user.Username) == 0)
                ClPresentationEnd();

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
            if (Connection.CurrentGroup.settings.NConnected)
                Notification.Show("User connected", user.Username + " joined", NotificationType.Connection);
        }

        private void OnUserDisconnect(User user)
        {
            if (Connection.CurrentGroup.settings.NDisconnected)
                Notification.Show("User disconnected", user.Username + " left", NotificationType.Connection);
        }

        public void Say(Message msg)
        {
            ChatController.RecieveMessage(msg);
            lock (syncObj)
            {
                foreach (ISharePCallback callback in users.Values)
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
            result.Add("NumberOfUsers", Connection.CurrentGroup.GetUsersCount().ToString()); 
            result.Add("Password", Connection.CurrentGroup.passwordProtected.ToString());
            result.Add("Download", Connection.CurrentGroup.settings.Download.ToString());
            result.Add("ViewersPresent", Connection.CurrentGroup.settings.Viewerspresent.ToString());
            result.Add("Chat", Connection.CurrentGroup.settings.Chat.ToString());
            if (Connection.CurrentGroup.navigation == GroupNavigation.Backwards)
                result.Add("GroupNavigation", "Backwards");
            else if (Connection.CurrentGroup.navigation == GroupNavigation.BothDirections)
                result.Add("GroupNavigation", "Both");
            else if (Connection.CurrentGroup.navigation == GroupNavigation.FollowOnly)
                result.Add("GroupNavigation", "Follow");
            return result;
        }

        public byte[] RequestSlide(int slide)
        {
            try
            {
                string pathToSlide = Helper.GetCurrentFolder() + @"tout\" + slide.ToString() + ".dat";
                byte[] buffer = File.ReadAllBytes(pathToSlide);
                return buffer;
            }
            catch (Exception ex)
            {
                Log.LogException(ex, "Failed to send slide");
                return null;
            }

        }

        public Presentation RequestCurrentPresentation()
        {
            return Connection.CurrentPresentation;
        }

        public void ViewerChangeFocus(bool focus, User user)
        {
            if (!focus)
                PresentationController.MarkCheater(user);
            else
                PresentationController.MarkNotCheater(user);
        }

        public async void ClPresentationStarted(Presentation presentation, User user)
        {
            Connection.CurrentPresentation = presentation;
            await Task.Factory.StartNew(() => LoadSlides());
            OnPresentationStart(presentation);
            Connection.FormMenu.OnPresentationStart();
        }

        public void LoadSlides()
        {
            lock (syncObj)
            {
                ISharePCallback presentationHost = null;
                if (SearchUsersByName(Connection.CurrentPresentation.Author))
                {
                    foreach (User key in users.Keys)
                    {
                        if (key.Username.CompareTo(Connection.CurrentPresentation.Author) == 0)
                            presentationHost = users[key];
                    }
                }
                if (presentationHost == null)
                {
                    Log.LogInfo("Can't load presentation. Host not found");
                    return;
                }

                DirectoryInfo din;
                DirectoryInfo dout;
                string path = Helper.GetCurrentFolder() + "tout/";
                string innerPath = Helper.GetCurrentFolder() + "tin/";
                if (!Directory.Exists(path))
                {
                    din = Directory.CreateDirectory(path);
                }
                else
                {
                    din = new DirectoryInfo(path);
                    foreach (FileInfo file in din.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in din.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }
                din.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                if (!Directory.Exists(innerPath))
                {
                    dout = Directory.CreateDirectory(innerPath);
                }
                else
                {
                    dout = new DirectoryInfo(innerPath);
                    foreach (FileInfo file in dout.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in dout.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }
                dout.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                try
                {
                    Log.LogInfo("[Server]Start loading slides (" + Connection.CurrentPresentation.SlidesTotal + ")");
                    for (int i = 1; i <= Connection.CurrentPresentation.SlidesTotal; i++)
                    {
                        byte[] file = presentationHost.ClRequestSlide(i);
                        FileStream fileStream = new FileStream(path + (i.ToString() + ".dat"), FileMode.Create, FileAccess.ReadWrite);
                        fileStream.Write(file, 0, file.Length);
                        FileStream fileStreamInner = new FileStream(innerPath + (i.ToString() + ".dat"), FileMode.Create, FileAccess.ReadWrite);
                        fileStreamInner.Write(file, 0, file.Length);
                    }
                }
                catch (Exception ex)
                {
                    Log.LogException(ex, "[Server]Error loading slide");
                }
            }
        }

        public void ClPresentationNextSlide(int slide)
        {
            OnPresentationNextSlide(slide);
            Connection.CurrentPresentation.CurrentSlide = slide;
            if (ViewerController.IsWorking)
                ViewerController.LoadSlide(slide);
        }

        public void ClPresentationEnd()
        {
            OnPresentationEnd();
            if (ViewerController.IsWorking)
                ViewerController.EndPresentation();
            Connection.CurrentPresentation = null;
            Connection.FormMenu.OnPresentationFinished();
        }
    }


    public static class ServerController
    {
        public static Group MyGroup
        {
            get;
            set;
        }

        static ServiceHost SelfHost;

        public static bool StartServer()
        {
            bool result = false;

            string ipBase = Helper.GetMyIP();

            Uri tcpAdrs = new Uri("net.tcp://" + ipBase + ":8000/ShareP/");
            Uri httpAdrs = new Uri("http://" + ipBase + ":8001/ShareP/");

            Uri[] baseAdresses = { tcpAdrs, httpAdrs };

            SharePService sharePService = new SharePService();
            SelfHost = new ServiceHost(
                            sharePService, baseAdresses);

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
                result = false;
                Log.LogException(e);
            }
            finally
            {
                if (SelfHost.State == CommunicationState.Opened)
                {
                    result = true;
                    Log.LogInfo("Server opened on " + ipBase);
                }
            }

            return result;

        }

        public static void OnPresentationStart(Presentation presentation)
        {
            ((SharePService)SelfHost.SingletonInstance).OnPresentationStart(presentation);
        }

        public static void OnPresentationNextSlide(int slide)
        {
            ((SharePService)SelfHost.SingletonInstance).OnPresentationNextSlide(slide);
        }

        public static void OnPresentationEnd()
        {
            ((SharePService)SelfHost.SingletonInstance).OnPresentationEnd();
        }

        public static void OnGroupClose()
        {
            ((SharePService)SelfHost.SingletonInstance).OnGroupClose();
        }

        public static void OnGroupSettingsChanged()
        {
            ((SharePService)SelfHost.SingletonInstance).OnGroupSettingsChanged();
        }

        public static void SendMessage(Message msg)
        {
            ((SharePService)SelfHost.SingletonInstance).Say(msg);
        }


        public static void StopServer()
        {
            if (SelfHost != null)
            {
                try
                {
                    SelfHost.Abort();
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
