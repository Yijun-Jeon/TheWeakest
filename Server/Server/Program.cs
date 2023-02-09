using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
    internal class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void FlushRoom()
        {
            Room.Push(() => { Room.Flush(); });
            // 0.25초마다 JobQueue Flush 예약
            JobTimer.Instance.Push(FlushRoom, 250);
            Console.WriteLine(SessionManager.Instance.Count());
        }

        static void Main(string[] args)
        {
            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7000);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            Console.WriteLine("Listening...");

            FlushRoom();
            while (true)
            {
                // 실행할 일감이 있는지만 계속 검사
                JobTimer.Instance.Flush();
            }
        }
    }
}
