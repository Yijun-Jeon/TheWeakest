using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
    // TEST Class
    class Zombie
    {
        public ushort rank;
        public double speed;
        public string name;
        public List<int> kills;
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            Zombie zombie = new Zombie()
            {
                rank = 1,
                speed = 3.1f,
                name = "Yijun",
                kills = new List<int>() { 1, 2, 3 }
            };

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            byte[] rankBuffer = BitConverter.GetBytes(zombie.rank);
            byte[] speedBuffer = BitConverter.GetBytes(zombie.speed);
            Array.Copy(rankBuffer,0,openSegment.Array,openSegment.Offset,rankBuffer.Length);
            Array.Copy(speedBuffer, 0,openSegment.Array,openSegment.Offset + rankBuffer.Length, speedBuffer.Length);
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(rankBuffer.Length + speedBuffer.Length);

            Send(sendBuff);

            Thread.Sleep(100);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }

    internal class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7000);

            _listener.Init(endPoint, () => { return new GameSession(); });

            Console.WriteLine("Listening...");
            while (true)
            {
                ;
            }
        }
    }
}
