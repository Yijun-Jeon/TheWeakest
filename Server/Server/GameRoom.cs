using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    internal class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        // 입장
        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }
        // 퇴장
        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
            }

        }

        // 모든 클라에게 채팅 전달
        public void BroadCast(ClientSession session, string chat)
        {
            lock (_lock)
            {
                S_Chat packet = new S_Chat();
                packet.playerId = session.SessionId;
                packet.chat = chat;
                ArraySegment<byte> sendBuff = packet.Write();

                foreach (ClientSession s in _sessions)
                    s.Send(sendBuff);
            }
        }
    }
}
