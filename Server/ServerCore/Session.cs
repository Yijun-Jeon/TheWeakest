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
        object _lock = new object();   

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _pending = false;

        public void Start(Socket socket)
        {
            _socket = socket;

            // recv
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            // send
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

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

        // Receive 와 달리 요청시 바로 처리
        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                // 전송 가능
                if (_pending == false)
                    RegisterSend();
            }
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

        public void RegisterSend()
        {
            // 전송 중으로 변경
            _pending = true;

            byte[] buff = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buff,0,buff.Length);

            bool pending = _socket.SendAsync(_sendArgs);
            // 전송 가능
            if(pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        public void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                // 전송할 바이트 유효 검사
                if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        if (_sendQueue.Count > 0)
                            RegisterSend();
                        else
                            _pending = false; // 전송 가능으로 변경
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Session OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }
        #endregion
    }
}
