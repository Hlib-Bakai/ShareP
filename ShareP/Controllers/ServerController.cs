using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace ShareP.Controllers
{

    [ServiceContract(Namespace = "http://ShareP")]
    public interface IShareP
    {
        [OperationContract]
        Dictionary<String, String> RequestServerInfo();
        [OperationContract]
        bool ClientConnect(Dictionary<String, String> clientInfo);
    }

    public class SharePService : IShareP
    {
        public bool ClientConnect(Dictionary<string, string> clientInfo)
        {
            return true;
        }

        public Dictionary<string, string> RequestServerInfo()
        {
            var result = new Dictionary<string, string>();
            result.Add("GroupName", "Diploma Seminar");
            result.Add("HostName", "Hlib Bakai");
            result.Add("NumberOfUsers", "20");
            result.Add("Password", "True");
            return result;
        }
    }


    class ServerController
    {

        ServiceHost SelfHost;

        public void StartServer()
        {
            string ipBase = getIPAddress();

            Uri BaseAddress = new Uri("http://" + ipBase + ":8000/ShareP/Service");

            SelfHost = new ServiceHost(typeof(SharePService), BaseAddress);
            try
            {
                SelfHost.AddServiceEndpoint(
                        typeof(IShareP),
                        new BasicHttpBinding(),
                        "SharePService");

                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                SelfHost.Description.Behaviors.Add(smb);

                SelfHost.Open();

                Log.LogInfo(String.Format("Server opened on: {0}", BaseAddress.ToString()));
            }
            catch (CommunicationException ce)
            {
                Log.LogException(ce);
                SelfHost.Abort();
            }
        }

        public void StopServer()
        {
            if (SelfHost.State == CommunicationState.Opened || SelfHost.State == CommunicationState.Opening)
                SelfHost.Close();
        }

        private static string getIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }
    }
}
