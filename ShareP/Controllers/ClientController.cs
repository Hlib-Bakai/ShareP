using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ShareP.Server;
using System.IO;
using System.ComponentModel;

namespace ShareP
{
    public class ClientController : ISharePCallback
    {
        SharePClient client = null;
        string rcvFilesPath = "";
        private delegate void FaultedInvoker();
        List<User> onlineUsers = new List<User>();
        public BackgroundWorker downloadingWorker = null;

        public ClientController()
        {
            rcvFilesPath = Helper.GetCurrentFolder() + "tin/";
        }

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

        public void DownloadPresentationSlidesOnBackground(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            DirectoryInfo di;
            string path = rcvFilesPath;
            if (!Directory.Exists(path))
            {
                di = Directory.CreateDirectory(path);
            }
            else
            {
                di = new DirectoryInfo(path);
            }
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            try
            {
                Log.LogInfo("Start loading slides (" + Connection.CurrentPresentation.SlidesTotal + ")");
                for (int i = 1; i <= Connection.CurrentPresentation.SlidesTotal; i++)
                {
                    byte[] file = client.RequestSlide(i);
                    FileStream fileStream = new FileStream(rcvFilesPath + (i.ToString() + ".dat"), FileMode.Create, FileAccess.ReadWrite);
                    fileStream.Write(file, 0, file.Length);
                    //downloadingWorker.ReportProgress((i / Connection.CurrentPresentation.SlidesTotal) * 100);
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex, "Error loading slide");
            }
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


        public Connection.ConnectionResult EstablishClientConnection(string ip, byte[] password = null)
        {
            if (client == null)
            {
                try
                {
                    InstanceContext instanceContext = new InstanceContext(this);
                    client = new SharePClient(instanceContext);
                    string servicePath = client.Endpoint.ListenUri.AbsolutePath;


                    client.Endpoint.Address = new EndpointAddress("net.tcp://" + ip + ":8000" + servicePath);

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
                        Connection.CurrentPresentation = client.RequestCurrentPresentation();
                        return Connection.ConnectionResult.Success;
                    }
                    else
                        return Connection.ConnectionResult.WrongPassword;
                }
                catch (Exception e)
                {
                    client = null;
                    Log.LogException(e, "Error during connection.");
                    return Connection.ConnectionResult.Error;
                }
            }
            return Connection.ConnectionResult.Error;
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
            client = null;
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
            Connection.OnUserJoin(user);
        }

        public void UserLeave(User user)
        {
            Connection.OnUserLeave(user);
        }


        public void PresentationNextSlide(int slide)
        {
            Connection.CurrentPresentation.CurrentSlide = slide;

            if (ViewerController.IsWorking)
                ViewerController.LoadSlide(slide);
        }

        public void PresentationEnd()
        {
            if (ViewerController.IsWorking)
                ViewerController.EndPresentation();
            Connection.CurrentPresentation = null;
            Connection.FormMenu.OnPresentationFinished();
        }

        public void PresentationStarted(Presentation presentation)
        {
            Connection.CurrentPresentation = presentation;
            Connection.FormMenu.OnPresentationStart();
        }

        public void GroupClose()
        {
            Connection.GroupClosed();
        }

        public void KickUser()
        {
            // TODO ?
        }

        public void GroupSettingsChanged(Dictionary<string, string> newSettings)
        {
            if (newSettings.ContainsKey("GroupName"))
                Connection.CurrentGroup.name = newSettings["GroupName"];
            if (newSettings.ContainsKey("HostName"))
                Connection.CurrentGroup.hostName = newSettings["HostName"];
            if (newSettings.ContainsKey("Download"))
                Connection.CurrentGroup.settings.Download = (newSettings["Download"].CompareTo("True") == 0) ? true : false;
            if (newSettings.ContainsKey("ViewersPresent"))
                Connection.CurrentGroup.settings.Viewerspresent = (newSettings["ViewersPresent"].CompareTo("True") == 0) ? true : false;
            if (newSettings.ContainsKey("GroupNavigation"))
            {
                if (newSettings["GroupNavigation"].CompareTo("Backwards") == 0)
                    Connection.CurrentGroup.navigation = GroupNavigation.Backwards;
                else if (newSettings["GroupNavigation"].CompareTo("Both") == 0)
                    Connection.CurrentGroup.navigation = GroupNavigation.BothDirections;
                else if (newSettings["GroupNavigation"].CompareTo("Follow") == 0)
                    Connection.CurrentGroup.navigation = GroupNavigation.FollowOnly;
            }
        }
    }
}
