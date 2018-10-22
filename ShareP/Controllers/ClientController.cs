using ShareP.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ShareP.Controllers
{
    public class ClientController
    {
        FormSearchServers m_formSearchServers;
        private int m_startedPing;
        private int m_finishedPing;

        public async void FindServersAsync(FormSearchServers form)
        {
            m_formSearchServers = form;
            await Task.Run(() => SearchTask());
        }

        private void SearchTask()
        {
            string ipBase = Helper.GetMyIP();
            string[] ipParts = ipBase.Split('.');
            ipBase = ipParts[0] + "." + ipParts[1] + "." + ipParts[2] + ".";
            m_startedPing = 0;
            m_finishedPing = 0;

            //Console.WriteLine("Started");
            for (int i = 1; i < 255; i++)
            {
                string ip = ipBase + i.ToString();

                Ping p = new Ping();
                p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
                p.SendAsync(ip, 100, ip);
                m_startedPing++;
            }
        }

        void p_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            m_finishedPing++;
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {

                string name;
                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                    name = hostEntry.HostName;
                    SharePClient client = new SharePClient(new BasicHttpBinding(), new EndpointAddress("http://" + ip + ":8000/ShareP/Service/SharePService"));
                    var serverInfo = client.RequestServerInfo();
                    bool pass = (serverInfo["Password"].CompareTo("True") == 0) ? true : false; // CHANGE THIS STRING TO BOOL
                    m_formSearchServers.AddGroup(new Group { name = serverInfo["GroupName"], hostName = serverInfo["HostName"], hostIp = ip, passwordProtected = pass });
                }
                catch (Exception ex)
                {
                    Log.LogException(ex);
                }
            }

            if (m_startedPing - m_finishedPing == 0)
            {
                m_formSearchServers.SearchStopped();
            }
        }
    }
}
