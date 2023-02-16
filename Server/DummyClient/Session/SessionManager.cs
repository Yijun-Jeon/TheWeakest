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
        Random _rand = new Random();

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
                    C_Move movePacket = new C_Move();
                    movePacket.posX = _rand.Next(-9, 9);
                    movePacket.posY = _rand.Next(-4, 4);
                    movePacket.posZ = 0;

                    session.Send(movePacket.Write());
                }
            }
        }
    }
}
