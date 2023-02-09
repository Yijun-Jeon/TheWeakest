using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient
{
    internal class SessionManager
    {
        #region SeingleTon
        static SessionManager _instance = new SessionManager();
        public static SessionManager Instance { get { return _instance; } }
        #endregion

        List<ServerSession> _sessions = new List<ServerSession>();
        object _lock = new object();

        // Session 생성
        public ServerSession Generate()
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                _sessions.Add(session);
                return session;
            }
        }

        // 모든 Session이 서버로 메시지 보냄
        public void SendForEach()
        {
            lock (_lock)
            {
                foreach (ServerSession session in _sessions)
                {
                    C_Chat packet = new C_Chat();
                    packet.chat = "Hi Server!";

                    ArraySegment<byte> sendBuff = packet.Write();
                    session.Send(sendBuff);
                }
            }
        }
    }
}
