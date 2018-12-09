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
        ConnectionResult Connect(User user, byte[] password);

        [OperationContract]
        ConnectionResult Reconnect(User user);

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

        [OperationContract]
        List<User> RequestUsersList();


        // Client presentations
        [OperationContract]
        bool RequestPresentationStart();

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
        void RefreshUsers(List<User> users); 

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

        List<User> usersToRemove = new List<User>();
        List<User> usersToReconnect = new List<User>();

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
                        usersToRemove.Add(key);
                    }
                }
                RemoveUsers();
            }
        }

        public void OnPresentationNextSlide(int slide)
        {
            lock (syncObj)
            {
                foreach (User key in users.Keys)
                {
                    ISharePCallback callback = users[key];
                    if (key.Username.CompareTo(Connection.CurrentPresentation.Author) == 0)
                        continue;
                    try
                    {
                        callback.PresentationNextSlide(slide);
                    }
                    catch (Exception ex)
                    {
                        Log.LogException(ex, "OnPresentationNextSlide Service");
                        usersToRemove.Add(key);
                    }
                }
                RemoveUsers();
            }
        }

        public void OnPresentationEnd()
        {
            lock (syncObj)
            {
                foreach (User key in users.Keys)
                {
                    ISharePCallback callback = users[key];
                    if (key.Username.CompareTo(Connection.CurrentPresentation.Author) == 0)
                        continue;
                    try
                    {
                        callback.PresentationEnd();
                    }
                    catch (Exception ex)
                    {
                        Log.LogException(ex, "OnPresentationEnd Service");
                        usersToRemove.Add(key);
                    }
                }
                RemoveUsers();
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
                        usersToRemove.Add(key);
                    }
                }
                RemoveUsers();
            }
        }
        
        void CallBackFaulted(object sender, EventArgs e)
        {
            Log.LogInfo("Callback faulted");
            ISharePCallback callback = sender as ISharePCallback;
            if (callback != null)
            {
                if (users.ContainsValue(callback))
                {
                    ICommunicationObject commObj = callback as ICommunicationObject;
                    if (commObj != null)
                    {
                        //remove the reference to the event handle to help memory cleanup
                        commObj.Closed -= new EventHandler(CallBackFaulted);
                    }
                    usersToReconnect.Add(users.FirstOrDefault(x => x.Value == callback).Key);
                    Disconnect(users.FirstOrDefault(x => x.Value == callback).Key);
                }
            }
        }


        /// IShareP

        public ConnectionResult Connect(User user, byte[] password)
        {
            if (!users.ContainsValue(CurrentCallback))
            {
                lock (syncObj)
                {
                    if (SearchUsersByName(user.Username))
                    {
                        Log.LogInfo("Username already exists: " + user.Username);
                        return ConnectionResult.UsernameExists;
                    }

                    if (Connection.CurrentGroup.passwordProtected && !Helper.CompareByteArrays(password, Connection.CurrentGroup.password))
                    {
                        Log.LogInfo("Wrong password. User: " + user.Username);
                        return ConnectionResult.WrongPassword;
                    }

                    Connection.CurrentGroup.AddUser(user);
                    OnUserConnect(user);
                    PresentationController.UserConnected(user);

                    foreach (User key in users.Keys)
                    {
                        ISharePCallback callback = users[key];
                        try
                        {
                            callback.RefreshUsers(Connection.CurrentGroup.userList);
                            callback.UserJoin(user);
                        }
                        catch
                        {
                            usersToRemove.Add(key);
                            Log.LogInfo("Callback faulted during Connect: " + key.Username);
                        }
                    }
                    RemoveUsers();
                    users.Add(user, CurrentCallback);
                    ICommunicationObject commObj = CurrentCallback as ICommunicationObject;
                    if (commObj != null)
                    {
                        commObj.Closed += new EventHandler(CallBackFaulted);
                        commObj.Faulted += new EventHandler(CallBackFaulted);
                    }
                }

                Log.LogInfo("User connected: " + user.Username);
                return ConnectionResult.Success;
            }
            Log.LogInfo("Refused connection of: " + user.Username);
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
                        foreach (User key in users.Keys)
                        {
                            ISharePCallback callback = users[key];
                            try {
                                callback.RefreshUsers(Connection.CurrentGroup.userList);
                                callback.UserLeave(user);
                            } catch
                            {
                                usersToRemove.Add(key);
                            }
                        }
                        RemoveUsers();
                    }
                    return;
                }
            }
        }

        public ConnectionResult Reconnect(User user)
        {
            if (!users.ContainsValue(CurrentCallback))
            {
                lock (syncObj)
                {
                    bool exists = false;
                    foreach(var item in usersToReconnect)
                    {
                        if (item.Username.CompareTo(user.Username) == 0 &&
                            item.IP == user.IP)
                            exists = true;
                    }
                    if (!exists)
                        return ConnectionResult.Error;
                    Connection.CurrentGroup.AddUser(user);
                    PresentationController.UserConnected(user);

                    foreach (User key in users.Keys)
                    {
                        ISharePCallback callback = users[key];
                        try
                        {
                            callback.RefreshUsers(Connection.CurrentGroup.userList);
                        }
                        catch
                        {
                            usersToRemove.Add(key);
                            Log.LogInfo("Callback faulted during Reconnect: " + key.Username);
                        }
                    }
                    RemoveUsers();
                    users.Add(user, CurrentCallback);
                    ICommunicationObject commObj = CurrentCallback as ICommunicationObject;
                    if (commObj != null)
                    {
                        commObj.Closed += new EventHandler(CallBackFaulted);
                        commObj.Faulted += new EventHandler(CallBackFaulted);
                    }
                }

                Log.LogInfo("User reconnected: " + user.Username);
                return ConnectionResult.Success;
            }
            Log.LogInfo("Refused reconnection from: " + user.Username);
            return ConnectionResult.Error;
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
            Log.LogInfo("Message sent: " + msg.Time.ToLongTimeString());
            lock (syncObj)
            {
                foreach (User key in users.Keys)
                {
                    ISharePCallback callback = users[key];
                    try
                    {
                        callback.Receive(msg);
                        Log.LogInfo("Finished: " + DateTime.Now.ToLongTimeString());
                    }
                    catch
                    {
                        usersToRemove.Add(key);
                        Log.LogInfo("Users kicked: " + DateTime.Now.ToLongTimeString());
                    }
                }
                RemoveUsers();
            }
        }

        public void IsWriting(User user)
        {
            lock (syncObj)
            {
                foreach (User key in users.Keys)
                {
                    ISharePCallback callback = users[key];
                    try
                    {
                        callback.IsWritingCallback(user);
                    }
                    catch
                    {
                        usersToRemove.Add(key);
                    }
                }
                RemoveUsers();
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
            Connection.FormMenu.StopPresentationReservedTimer();
            Connection.ReservePresentation = false;
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

        public List<User> RequestUsersList()
        {
            if (Connection.CurrentGroup != null)
                return Connection.CurrentGroup.userList;
            else
                return null;
        }

        private void RemoveUsers()
        {
            for (int i = 0; i < usersToRemove.Count; i++)
            {
                var key = usersToRemove[i];
                users.Remove(key);
                Disconnect(key);
            }
            usersToRemove.Clear();
        }

        public bool RequestPresentationStart()
        {
            lock (syncObj)
            {
                bool answer = !Connection.ReservePresentation;
                Log.LogInfo("User requested presentation start. Answer: " + answer);
                if (answer)
                {
                    Connection.ReservePresentation = true;
                    Connection.FormMenu.StartPresentationReservedTimer();
                }
                return answer;
            }
        }
    }


    public static class ServerController
    {
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

            ServiceThrottlingBehavior throttle; // Performance 
            throttle = SelfHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (throttle == null)
            {
                throttle = new ServiceThrottlingBehavior();
                throttle.MaxConcurrentCalls = 100;              //Here as well
                throttle.MaxConcurrentSessions = 100;           //--
                SelfHost.Description.Behaviors.Add(throttle);
            }

            tcpBinding.ReceiveTimeout = new TimeSpan(24, 0, 0); 
            tcpBinding.ReliableSession.Enabled = true;
            tcpBinding.ReliableSession.InactivityTimeout = new TimeSpan(0, 0, 5); // Disconnect after 5 seconds of inactivity 

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
