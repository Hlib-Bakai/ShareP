using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace WCF
{
    [ServiceContract(Namespace = "http://WCF")]
    public interface ICalculator
    {
        [OperationContract]
        bool SendMessage(string message);
    }

    public class CalculatorService : ICalculator
    {
        public bool SendMessage(string message)
        {
            Console.WriteLine("Received message: '{0}'", message);
            return true;
        }
    }

    class Server
    {
        static void Main(string[] args)
        {
        
            string ipBase = getIPAddress();

            Uri BaseAddress = new Uri("http://" + ipBase + ":8000/WCF/Service");

            ServiceHost SelfHost = new ServiceHost(typeof(CalculatorService), BaseAddress);
            try
            {
                SelfHost.AddServiceEndpoint(
                        typeof(ICalculator),
                        new WSHttpBinding(),
                        "CalculatorService");

                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                SelfHost.Description.Behaviors.Add(smb);

                SelfHost.Open();
                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <ENTER> to terminate service.");
                Console.WriteLine();
                Console.ReadLine();

                // Close the ServiceHostBase to shutdown the service.
                SelfHost.Close();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occured: {0}", ce.Message);
                Console.ReadLine();
                SelfHost.Abort();
            }
        }

        public static string getIPAddress()
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
