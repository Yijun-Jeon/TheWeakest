using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Server
{
    internal class Program
    {
        static Listener _listener = new Listener();

        static void FlushRoom()
        {
            // 0.25초마다 JobQueue Flush 예약
            JobTimer.Instance.Push(FlushRoom, 250);
        }

        static void CountClient()
        {
            Console.WriteLine(SessionManager.Instance.Count());
            JobTimer.Instance.Push(CountClient, 1000);
        }

        static void Main(string[] args)
        {
            RoomManager.Instance.Add(1);

            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            // 로컬 서버 IP 주소 -> 포트포워드 설정 필요
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7000);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            Console.WriteLine("Listening...");

            FlushRoom();
            CountClient();
            while (true)
            {
                // 실행할 일감이 있는지만 계속 검사
                JobTimer.Instance.Flush();
            }
        }
    }
}
