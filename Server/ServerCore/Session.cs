using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public class Session
    {
        Socket _socket;
        int _disconnected = 0;

        public void Start(Socket socket)
        {
            _socket = socket;

            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            
            // 버퍼 설정
            recvArgs.SetBuffer(new byte[1024],0, 1024);
            RegisterRecv(recvArgs);
        }

        public void Disconnect()
        {
            // 중복 처리 방지
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        public void Send(byte[] sendBuff)
        {
            _socket.Send(sendBuff);
        }

        #region Network

        public void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
                OnRecvCompleted(null, args);
        }

        public void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            // Receive 바이트 유효 검사
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData =  Encoding.UTF8.GetString(args.Buffer,args.Offset,args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");

                    // 다음 작업 등록
                    RegisterRecv(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Session OnRecvCompleted Failed {e.ToString()}");
                }
            }
            else
            {
                // TODO : Disconnect
            }
        }
        #endregion
    }
}
