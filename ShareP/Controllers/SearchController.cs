using ShareP.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShareP.Controllers
{
    public class SearchController
    {
        FormSearchServers m_formSearchServers;
        private int m_startedPing;
        private int m_finishedPing;
        private int m_replies;
        private CancellationToken ct;
        private CancellationTokenSource ts;

        private bool searchFinished = false;

        private object _lock = new object();

        private Ping[] pings;


        public void FindServersAsync(FormSearchServers form)
        {
            m_formSearchServers = form;
            ts = new CancellationTokenSource();
            ct = ts.Token;

            Task.Factory.StartNew(() => SearchTask());
        }

        public void StopSearch()
        {
            try
            {
                ts.Cancel();
            }
            catch { }
        }

        private void SearchTask()
        {
            StopSearch();

            searchFinished = false;

            pings = new Ping[65024];

            ts = new CancellationTokenSource();
            ct = ts.Token;

            string ipBase = Helper.GetMyIP();
            string[] ipParts = ipBase.Split('.');
            ipBase = ipParts[0] + "." + ipParts[1] + "." + ipParts[2] + ".";
            string ipBaseSmaller = ipParts[0] + "." + ipParts[1] + ".";
            m_startedPing = 0;
            m_finishedPing = 0;
            m_replies = 0;

            Log.LogInfo("Ping started");

            // Ping "small" subnetwork
            // 192.168.0.x
            for (int i = 1; i < 255; i++)
            {
                string ip = ipBase + i.ToString();

                Ping p = new Ping();
                p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
                p.SendAsync(ip, 100, ip);
                m_startedPing++;
            }

            Log.LogInfo("Deep scan started");

            // Ping "big" subnetwork
            // 192.168.x.x
            for (int i = 0; i < 255; i++)
            {
                string ipBaseThree = ipBaseSmaller + i.ToString() + ".";
                for (int j = 1; j < 255; j++)
                {
                    string ip = ipBaseThree + j.ToString();
                    Ping p = new Ping();
                    p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
                    p.SendAsync(ip, 1, ip);
                    m_startedPing++;
                    if (ct.IsCancellationRequested)
                    {
                        Log.LogInfo("Search stopped");
                        break;
                    }
                }
                if (ct.IsCancellationRequested)
                {
                    Log.LogInfo("Search stopped");
                    break;
                }
            }

            Log.LogInfo("Started pings: " + m_startedPing.ToString());
        }

        void p_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            lock (_lock)
            {
                m_finishedPing++;
            }

            (sender as IDisposable).Dispose();

            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                lock (_lock)
                {
                    m_replies++;
                }
                var result = Connection.GetServiceOnIP(ip);
                if (result != null)
                {
                    bool pass = (result["Password"].CompareTo("True") == 0) ? true : false;
                    m_formSearchServers.AddGroup(new Group { name = result["GroupName"], hostName = result["HostName"], hostIp = ip, passwordProtected = pass }, result["NumberOfUsers"]);
                }
            }

            if (m_finishedPing == 254)
                m_formSearchServers.FastSearchFinished();


            if (/*m_startedPing - m_finishedPing == 0 && !ct.IsCancellationRequested*/m_finishedPing == 65024)
            {
                lock (_lock)
                {
                    try
                    {
                        if (!searchFinished)
                        {
                            searchFinished = true;
                            Log.LogInfo("Search finished. Scanned: " + m_finishedPing);
                            Log.LogInfo("Hosts answered: " + m_replies);
                            m_formSearchServers.SearchStopped();
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
