﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ShareP.Server;
using System.IO;
using System.ComponentModel;
using System.Threading;

namespace ShareP
{
    public class ClientController : ISharePCallback
    {
        SharePClient client = null;
        string rcvFilesPath = "";
        private delegate void FaultedInvoker();
        public BackgroundWorker downloadingWorker = null;
        private bool faulted = false;

        public ClientController()
        {
            rcvFilesPath = Helper.GetCurrentFolder() + "tin/";
        }
        
        void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        {
            if (faulted)
                return;
            Log.LogInfo("Channel faulted");
            faulted = true;
            try
            {
                if (!ReconnectOnFaultAsync().Result)
                    Connection.GroupClosed(true);
            }
            catch (Exception ex)
            {
                Log.LogException(ex, "Exception during faulted");
                Connection.GroupClosed(true);
            }
        }

        public void DownloadPresentationSlidesOnBackground(object sender, DoWorkEventArgs e)
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
                    if (file == null)
                    {
                        i--;
                        continue;
                    }
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
                      new EventHandler(InnerDuplexChannel_Faulted);

                    ConnectionResult connectionResult = client.Connect(Connection.CurrentUser, password);
                    if (connectionResult == ConnectionResult.Success)
                    {
                        Log.LogInfo("Successfuly connected to " + ip);
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
            var cancellationToken = new CancellationTokenSource();
            var task = Task.Factory.StartNew(RecconectTask, cancellationToken.Token);
            Log.LogInfo("Starting reconecting task");
            if (await Task.WhenAny(task, Task.Delay(2000)) == task)
            {
                faulted = false;
                return true;
            }
            else
            {
                Log.LogInfo("Timeout of reconnecting");
                cancellationToken.Cancel();
            }
            faulted = false;
            return false;
        }


        public bool RecconectTask()
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
                  new EventHandler(InnerDuplexChannel_Faulted);

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
            return client.RequestServerInfo();
        }

        public void ReportFocusChanged(bool focus)
        {
            Log.LogInfo("Client controller: report focus " + focus);
            client.ViewerChangeFocus(focus, Connection.CurrentUser);
        }


        public void Disconnect()
        {
            if (client == null)
                return;

            if (client.State == CommunicationState.Faulted)
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

        public void ClPresentationStart(Presentation presentation)
        {
            if (client != null && client.State == CommunicationState.Opened)
                client.ClPresentationStarted(presentation, Connection.CurrentUser);
        }

        public void ClPresentationNextSlide(int slide)
        {
            if (client != null && client.State == CommunicationState.Opened)
                client.ClPresentationNextSlide(slide);
        }

        public void ClPresentationEnd()
        {
            if (client != null && client.State == CommunicationState.Opened)
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
            if (client != null)
                return new List<User>(client.RequestUsersList());
            else
                return null;
        }
    }
}
