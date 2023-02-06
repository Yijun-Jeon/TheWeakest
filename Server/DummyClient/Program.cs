using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace DummyClient
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected To {endPoint}");

            // TEST
            for(int i=0; i < 5; i++)
            {
                // Send
                byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello Server I'm DummyClient {i} ");
                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            ushort rank = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            double speed = BitConverter.ToDouble(buffer.Array,buffer.Offset + sizeof(ushort));
            
            //string recvData = Encoding.UTF8.GetString(buffer.Array,buffer.Offset,buffer.Count);
            
            Console.WriteLine($"[From Server] rank({rank}) speed({speed})");

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }

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
            connector.Connect(endPoint, () => { return new GameSession(); });
         

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
