using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace DummyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7000);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new ServerSession(); });
         

            while (true)
            {
                try
                {

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                // TEST
                Thread.Sleep(1000);
            }
        }
    }
}
