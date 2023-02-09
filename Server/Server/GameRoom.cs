using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    internal class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();
        JobQueue _jobQueue = new JobQueue();

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        // 입장
        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }
        // 퇴장
        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }

        // 모든 클라에게 채팅 전달
        public void BroadCast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = chat;
            ArraySegment<byte> sendBuff = packet.Write();

            _pendingList.Add(sendBuff);
        }

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            Console.WriteLine($"[Server] Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }
    }
}
