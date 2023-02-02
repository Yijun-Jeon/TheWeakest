using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

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

            // Tcp 소켓 생성
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 소켓 연결
                socket.Connect(endPoint);
                Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                // Send
                byte[] sendBuff = Encoding.UTF8.GetBytes("Hello Server I'm DummyClient");
                socket.Send(sendBuff);

                // Receive
                byte[] recvBuff = new byte[1024];
                int recvBytes = socket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff,0,recvBytes);
                Console.WriteLine($"[From Server] {recvData}");

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
