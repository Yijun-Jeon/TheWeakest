using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    internal class SessionManager
    {
        #region SingleTon
        static SessionManager _instance = new SessionManager();
        public static SessionManager Instance { get { return _instance; } }
        #endregion

        object _lock = new object();
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();

        // 발급 번호
        int _sessionId = 0;

        public int Count()
        {
            return _sessions.Count;
        }

        // Session 생성 인터페이스
        public ClientSession Generate()
        {
            lock (_lock)
            {
                int sessionId = ++_sessionId;

                ClientSession session = new ClientSession();
                session.SessionId = sessionId;
                _sessions.Add(sessionId, session);

                Console.WriteLine($"Connected : {sessionId}");

                return session;
            }
        }

        // Session 찾는 인터페이스
        public ClientSession Find(int id)
        {
            ClientSession session = null;
            lock (_lock)
            {
                if (_sessions.TryGetValue(id, out session))
                    return session;
            }

            return null;
        }

        // Session 제거 인터페이스
        public void Remove(ClientSession session)
        {
            lock (_lock)
            {
                if (_sessions.ContainsKey(session.SessionId))
                    _sessions.Remove(session.SessionId);
            }
        }
    }
}
