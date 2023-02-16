using ServerCore;
using System;
using System.Collections.Generic;

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
            // 플레이어 추가
            _sessions.Add(session);
            session.Room = this;

            // 신입생한테 모든 플레이어 목록 전송 - BroadCasting X
            S_PlayerList players = new S_PlayerList();
            foreach (ClientSession s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ
                });
            }
            session.Send(players.Write());

            // 신입생 입장을 모두에게 알림
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            Broadcast(enter.Write());
        }

        // 퇴장
        public void Leave(ClientSession session)
        {
            // 플레이어 제거
            _sessions.Remove(session);

            // 제거를 모두에게 알림
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionId;
            Broadcast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet)
        {
            // 좌표 바꿔주고
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            // 모두에게 알림
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
            Broadcast(move.Write());
        }

        // 모든 클라에게 패킷 전달
        public void Broadcast(ArraySegment<byte> segment)
        {
            _pendingList.Add(segment);
        }

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            //Console.WriteLine($"[Server] Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }
    }
}
