using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ShareP.Server;
using System.IO;
using System.ComponentModel;
using System.Threading;
using ShareP.Forms;

namespace ShareP
{
    public class ClientController : ISharePCallback
    {
        SharePClient client = null;
        string rcvFilesPath = "";
        private delegate void FaultedInvoker();
        public BackgroundWorker downloadingWorker = null;
        private bool faulted = false;
        private CancellationTokenSource cancellationToken;
        private FormReconnecting formReconnecting;

        public ClientController()
        {
            rcvFilesPath = Helper.GetCurrentFolder() + "tin/";
        }

        async void InnerDuplexChannel_FaultedAsync(object sender, EventArgs e)
        {
            if (faulted)
                return;
            Log.LogInfo("Channel faulted");
            faulted = true;
            try
            {
                await ReconnectOnFaultAsync();
            }
            catch (Exception ex)
            {
                Log.LogException(ex, "Exception during faulted");
                Connection.GroupClosed(true);
            }
        }

        public bool Faulted
        {
            get
            {
                return faulted;
            }
        }

        private bool CheckBeforeRequest()
        {
            if (client == null)
            {
                Log.LogInfo("Check before request: client is null");
                return false;
            }
            if (faulted)
            {
                Log.LogInfo("Check before request: client faulted");
                return false;
            }
            Log.LogInfo("Check before request: OK");
            return true;
        }

        public void DownloadPresentationSlidesOnBackground(object sender, DoWorkEventArgs e)
        {
            if (!CheckBeforeRequest())
                return;

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
                int startingSlide = ViewerController.LastSlideDownloaded + 1;
                Log.LogInfo("Start loading slides (" + Connection.CurrentPresentation.SlidesTotal + ") from slide #" + startingSlide);
                for (int i = startingSlide; i <= Connection.CurrentPresentation.SlidesTotal; i++)
                {
                    Log.LogInfo("Loading slide # " + i);
                    Stream stream = client.RequestSlide(i);
                    byte[] file;
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        file = ms.ToArray();
                    }
                    if (file == null)
                    {
                        Log.LogInfo("NULL when requesting slide");
                        i--;
                        continue;
                    }
                    Log.LogInfo("Slide loaded. Converting");
                    FileStream fileStream = new FileStream(rcvFilesPath + (i.ToString() + ".dat"), FileMode.Create, FileAccess.ReadWrite);
                    fileStream.Write(file, 0, file.Length);
                    Log.LogInfo("Slide converted");
                    ViewerController.LastSlideDownloaded = i;
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


        public ConnectionResult EstablishClientConnection(string ip, byte[] password = null)
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
                      new EventHandler(InnerDuplexChannel_FaultedAsync);

                    ConnectionResult connectionResult = client.Connect(Connection.CurrentUser, password);
                    if (connectionResult == ConnectionResult.Success)
                    {
                        Log.LogInfo("Successfuly connected to " + ip);
                        faulted = false;
                        Connection.CurrentPresentation = client.RequestCurrentPresentation();
                    }
                    else
                    {
                        client = null;
                    }
                    return connectionResult;
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

        public async Task<bool> ReconnectOnFaultAsync()
        {
            Log.LogInfo("Trying to reconnect");
            cancellationToken = new CancellationTokenSource();
            var task = Task.Factory.StartNew(ReconnectTask, cancellationToken.Token).ContinueWith(OnReconnectFinish);
            Log.LogInfo("Starting reconecting task");
            if (await Task.WhenAny(task, Task.Delay(3000)) != task)
            {
                Log.LogInfo("Timeout of fast reconnecting");
                ShowReconnectWindow();
                //cancellationToken.Cancel();
            }
            return false;
        }

        private void OnReconnectFinish(Task<bool> obj)
        {
            if (!faulted)
                return;
            Log.LogInfo("Reconnect finish");
            if (formReconnecting != null)
            {
                Log.LogInfo("Closing reconnect window");
                formReconnecting.Invoke(new Action(() => formReconnecting.Close()));
            }
            if (obj.Result == false)
            {
                Log.LogInfo("Reconnect returned false");
                faulted = true;
                Connection.GroupClosed(true);
            }
            else
            {
                Log.LogInfo("Reconnect successful");
                faulted = false;
                ViewerController.ResumeDownloading();
            }
        }

        private void ShowReconnectWindow()
        {
            if (!faulted || formReconnecting != null)
                return;
            Log.LogInfo("Showing reconnect window");
            Connection.FormMenu.RestoreWindow();
            int overlay = Helper.ShowOverlay();
            formReconnecting = new FormReconnecting();
            if (Connection.FormMenu.InvokeRequired)
                Connection.FormMenu.Invoke(new Action<FormMenu>((f) => formReconnecting.ShowDialog(f)), Connection.FormMenu);
            else
                formReconnecting.ShowDialog(Connection.FormMenu);
            Log.LogInfo("Reconnect window closed");
            Helper.HideOverlay(overlay);
            formReconnecting = null;
        }

        public void CancelReconnecting()
        {
            if (faulted && !cancellationToken.IsCancellationRequested)
            {
                cancellationToken.Cancel();
                Connection.GroupClosed(true);
            }
        }

        public bool ReconnectTask()
        {
            try
            {
                var ip = Connection.CurrentGroup.hostIp;

                InstanceContext instanceContext = new InstanceContext(this);
                client = new SharePClient(instanceContext);
                string servicePath = client.Endpoint.ListenUri.AbsolutePath;


                client.Endpoint.Address = new EndpointAddress("net.tcp://" + ip + ":8000" + servicePath);

                client.Open();

                Log.LogInfo("Connection reopened");

                client.InnerDuplexChannel.Faulted +=
                  new EventHandler(InnerDuplexChannel_FaultedAsync);

                ConnectionResult connectionResult = client.Reconnect(Connection.CurrentUser);
                if (connectionResult == ConnectionResult.Success)
                {
                    Log.LogInfo("Successfuly reconnected to " + ip);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                client = null;
                Log.LogException(e, "Error during reconnection.");
                return false;
            }
        }



        public Dictionary<string, string> RequestServerInfo()
        {
            if (!CheckBeforeRequest())
                return null;
            return client.RequestServerInfo();
        }

        public void ReportFocusChanged(bool focus)
        {
            if (!CheckBeforeRequest())
                return;
            Log.LogInfo("Client controller: report focus " + focus);
            client.ViewerChangeFocus(focus, Connection.CurrentUser);
        }


        public void Disconnect()
        {
            if (!CheckBeforeRequest())
            {
                client = null;
                return;
            }

            try
            {
                client.DisconnectAsync(Connection.CurrentUser);
            }
            catch { }

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
            ChatController.RecieveMessage(msg);
        }

        public void SendMessage(Message msg)
        {
            if (!CheckBeforeRequest())
                return;
            client.Say(msg);
        }

        public void RefreshUsers(User[] users)
        {
            Connection.CurrentGroup.userList = new List<User>(users);
            if (Connection.FormMenu.InvokeRequired)
                Connection.FormMenu.Invoke(new Action(() => Connection.FormMenu.FillChatUsersList()));
            else
                Connection.FormMenu.FillChatUsersList();
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
            if (Connection.CurrentPresentation != null)
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

        public bool ClRequestPresentationStart()
        {
            if (!CheckBeforeRequest())
                return false;
            return client.RequestPresentationStart();
        }
        
        public void ClPresentationStart(Presentation presentation)
        {
            if (!CheckBeforeRequest())
                return;
            client.ClPresentationStarted(presentation, Connection.CurrentUser);
        }

        public void ClPresentationNextSlide(int slide)
        {
            if (!CheckBeforeRequest())
                return;
            client.ClPresentationNextSlide(slide);
        }

        public void ClPresentationEnd()
        {
            if (!CheckBeforeRequest())
                return;
            client.ClPresentationEnd();
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

        public byte[] ClRequestSlide(int slide)
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

        public List<User> RequestUsersList()
        {
            if (!CheckBeforeRequest())
            {
                return null;
            }
            return new List<User>(client.RequestUsersList());
        }

    }
}
