using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Stuff
{
    class Stuff
    {
        static bool resolveNames = true;

        static void Main(string[] args)
        {
            string ipBase = getIPAddress();
            string[] ipParts = ipBase.Split('.');
            ipBase = ipParts[0] + "." + ipParts[1] + "." + ipParts[2] + ".";
            int ind = 0;
            while (!Console.ReadLine().Contains('q'))
            {
                Console.Clear();
                Console.WriteLine("Started again ({0})", ind);
                for (int i = 1; i < 255; i++)
                {
                    string ip = ipBase + i.ToString();

                    Ping p = new Ping();
                    p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
                    p.SendAsync(ip, 100, ip);
                }
                ind++;
            }
        }
    
        static void p_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                if (resolveNames)
                {
                    string name;
                    try
                    {
                        IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                        name = hostEntry.HostName;
                        //ServiceController[] services = ServiceController.GetServices(name);
                        //Console.WriteLine("=== Services on {0} ===", name);
                        //foreach(var service in services)
                        //{
                        //    Console.WriteLine("- {0} : {1}", service.ServiceType, service.ServiceName);
                        //}
                        //var myService = services.FirstOrDefault(s => s.ServiceName == "CalculatorService");
                        //Console.WriteLine("=== End === Found: {0}", (myService != null));
                        CalculatorClient client = new CalculatorClient(new BasicHttpBinding(), new EndpointAddress("http://" + ip + ":8000/WCF/Service/CalculatorService"));
                        client.SendMessage("Hello");
                        Console.WriteLine("! Message sent !");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("--Excepiton: {0}--", ex.Message);
                        if (ex.InnerException != null)
                            Console.WriteLine("--Inner: {0}--", ex.InnerException.Message);
                        name = "?";
                    }
                    Console.WriteLine("{0} ({1}) is up: ({2} ms)", ip, name, e.Reply.RoundtripTime);
                }
                else
                {
                    Console.WriteLine("{0} is up: ({1} ms)", ip, e.Reply.RoundtripTime);
                }
            }
            else if (e.Reply == null)
            {
                Console.WriteLine("Pinging {0} failed. (Null Reply object?)", ip);
            } 
            else
            {
                //Console.WriteLine("Pinging {0} failed. Status: {1}", ip, e.Reply.Status);
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
