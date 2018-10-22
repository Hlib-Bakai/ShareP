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
        bool ClientConnect(Dictionary<String, String> clientInfo, byte[] password);
    }

    public class SharePService : IShareP
    {
        public bool ClientConnect(Dictionary<string, string> clientInfo, byte[] password)
        {
            if (Connection.CurrentGroup.passwordProtected && !Helper.CompareByteArrays(password, Connection.CurrentGroup.password))
            {
                return false;
            }
            User user = new User();
            user.ChangeUsername(clientInfo["username"]);
            user.IP = clientInfo["IP"];
            Connection.CurrentGroup.AddUser(user);
            return true;
        }

        public bool ClientConnect(Dictionary<string, string> clientInfo)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> RequestServerInfo()
        {
            var result = new Dictionary<string, string>();
            result.Add("GroupName", Connection.CurrentGroup.name);
            result.Add("HostName", Connection.CurrentGroup.hostName);
            result.Add("NumberOfUsers", "20");
            result.Add("Password", Connection.CurrentGroup.passwordProtected.ToString());
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

        public static void StopServer()
        {
            if (SelfHost.State == CommunicationState.Opened || SelfHost.State == CommunicationState.Opening)
                SelfHost.Close();
        }
    }
}
