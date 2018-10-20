using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace WCF.Client
{
    class Client
    {
        static void Main(string[] args)
        {
            try
            {
                // Step 1: Create an endpoint address and an instance of the WCF client
                //CalculatorClient client = new CalculatorClient();
                CalculatorClient client = new CalculatorClient(new BasicHttpBinding(), new EndpointAddress("http://192.168.0.108:8000/WCF/Service/CalculatorService"));
                Console.WriteLine("Client is working. Write your message.");
                Console.WriteLine("Enter 'q' to terminate client.");

                // Step 2: Call the service operations.

                string outMessage = "";
                while ((outMessage = Console.ReadLine()).CompareTo("q") != 0)
                {
                    string recieved = client.SendMessage(outMessage) ? "Recipient got the message" : "Some problems occured";
                    Console.WriteLine(recieved);
                }

                // Step 3: Close the client gracefully.
                // This closes the connection and cleans up resources.
                client.Close();

                Console.WriteLine();
                
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Press <ENTER> to terminate client.");
            Console.ReadLine();
        }
    }
}
