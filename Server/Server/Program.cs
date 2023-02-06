using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
    // TEST Packet
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class GameSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Server] OnConnected: {endPoint}");

            //Packet packet = new Packet()
            //{
            //    size = 4,
            //    packetId = 7
            //};

            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            //byte[] sizeBuffer = BitConverter.GetBytes(packet.size);
            //byte[] idBuffer = BitConverter.GetBytes(packet.packetId);
            //Array.Copy(sizeBuffer, 0, openSegment.Array, openSegment.Offset, sizeBuffer.Length);
            //Array.Copy(idBuffer, 0, openSegment.Array, openSegment.Offset + sizeBuffer.Length, idBuffer.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(sizeBuffer.Length + idBuffer.Length);

            //Send(sendBuff);

            Thread.Sleep(100);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Server] OnDisconnected: {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            // 패킷 데이터 추출
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + sizeof(ushort));

            Console.WriteLine($"[From Client] packetId({packetId}) size({size})");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"[To Client] Transferred bytes : {numOfBytes}");
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
