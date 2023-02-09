using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        // 어떤 Session을 만들어 줄 지
        Func<Session> _sessionFactory;
        
        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            _listenSocket = new Socket(endPoint.AddressFamily,SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            // 소켓 바인드
            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(backlog);

            // register 개수만큼 문지기 등록
            for (int i = 0; i < register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                // 콜백으로 전달
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);
            }
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 기존 socket 초기화
            args.AcceptSocket = null;

            // 추후 처리 여부
            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
                OnAcceptCompleted(null, args);
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }
            // 다음 작업 등록
            RegisterAccept(args);
        }        
    }
}
