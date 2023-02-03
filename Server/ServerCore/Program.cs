using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    internal class Program
    {
        static Listener _listener = new Listener();

        static void onAcceptHandler(Socket clientSocket)
        {
            try
            {
                // Receive
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine($"[From Client] {recvData}");

                // Send
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to TheWeakest Server!");
                clientSocket.Send(sendBuff);

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void Main(string[] args)
        {
            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7000);

            // Tcp 소켓 생성
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listener.Init(endPoint, onAcceptHandler);

            Console.WriteLine("Listening...");
            while (true)
            {
                ;
            }
        }
    }
}
