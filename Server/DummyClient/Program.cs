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
            
            // 서버 공용 IP 주소
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("61.102.132.68"), 7000);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return SessionManager.Instance.Generate(); },10);
         

            while (true)
            {
                try
                {
                    // 서버쪽으로 모든 클라가 메시지를 보냄
                    SessionManager.Instance.SendForEach();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                // TEST
                Thread.Sleep(250);
            }
        }
    }
}
