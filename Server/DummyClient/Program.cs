using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace DummyClient
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
            Console.WriteLine($"[Client] Connected To {endPoint}");

            Packet packet = new Packet()
            {
                size = 4,
                packetId = 7
            };

            for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
                byte[] sizeBuffer = BitConverter.GetBytes(packet.size);
                byte[] idBuffer = BitConverter.GetBytes(packet.packetId);
                Array.Copy(sizeBuffer, 0, openSegment.Array, openSegment.Offset, sizeBuffer.Length);
                Array.Copy(idBuffer, 0, openSegment.Array, openSegment.Offset + sizeBuffer.Length, idBuffer.Length);
                ArraySegment<byte> sendBuff = SendBufferHelper.Close(sizeBuffer.Length + idBuffer.Length);

                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Client] OnDisconnected: {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            // 패킷 데이터 추출
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + sizeof(ushort));

            Console.WriteLine($"[From Server] packetId({packetId}) size({size})");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"[To Server] Transferred bytes : {numOfBytes}");
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
