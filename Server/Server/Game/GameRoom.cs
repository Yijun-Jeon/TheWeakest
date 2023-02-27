using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

namespace Server
{
    public class GameRoom
    {
        object _lock= new object();
        public int RoomId { get; set; }

        List<Player> _players = new List<Player>();

        public void EnterGame(Player newPlayer)
        {
            if (newPlayer == null)
                return;

            lock (_lock)
            {
                _players.Add(newPlayer);
                newPlayer.Room = this;

                // 본인에게 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    // 타인들 정보
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach(Player p in _players)
                    {
                        if(newPlayer != p)
                            spawnPacket.Players.Add(p.Info);
                    }
                    newPlayer.Session.Send(spawnPacket);

                }
                // 타인들에게 정보 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(newPlayer.Info);
                    foreach (Player p in _players)
                    {
                        if(newPlayer != p)
                            p.Session.Send(spawnPacket);
                    }
                }
            }
        }

        public void LeaveGame(int playerId)
        {
            lock (_lock)
            {
                Player player = _players.Find(p => p.Info.PlayerId == playerId);
                if (player == null)
                    return;

                _players.Remove(player);
                player.Room = null;

                // 본인에게 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
                // 타인들에게 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIds.Add(player.Info.PlayerId);
                    foreach(Player p in _players)
                    {
                        if (player != p)
                            p.Session.Send(despawnPacket);
                    }
                }
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach(Player p in _players)
                {
                    p.Session.Send(packet);
                }
            }
        }

        // 플레이어 이동 처리
        public void HandleMove(Player player,C_Move movePacket)
        {
            if (player == null)
                return;

            // 실질적인 정보 수정은 한 쓰레드만 실행되도록
            lock (_lock)
            {
                // TODO : 검증

                // 일단 서버에서 좌표 이동
                PlayerInfo info = player.Info;
                info.PosInfo = movePacket.PosInfo;

                // 다른 플레이어들에게 이동을 알려줌
                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerId = player.Info.PlayerId;
                resMovePacket.PosInfo = movePacket.PosInfo;

                Broadcast(resMovePacket);
            }
        }

        // 플레이어 공격 처리
        public void HandleAttack(Player player, C_Attack attackPacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                PlayerInfo info = player.Info;

                // 공격을 할 수 없는 상태
                if (info.PosInfo.State == PlayerState.Fake || info.PosInfo.State == PlayerState.Dead)
                    return;

                // TODO : 쿨타임, 상대방 공격력 체크
                
                // 공격 패킷 전송 
                S_Attack attack = new S_Attack();
                attack.PlayerId = player.Info.PlayerId;
                Broadcast(attack);

                // TODO : 둘 중 하나 사망 처리
            }
        }

        // 플레이어 죽은 척 처리
        public void HandleFake(Player player, C_Fake fakePacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                PlayerInfo info = player.Info;

                // 죽은 척 할 수 없는 상태
                if (info.PosInfo.State == PlayerState.Fake || info.PosInfo.State == PlayerState.Dead)
                    return;

                // TODO : 쿨타임

                // 죽은 척 패킷 전송
                S_Fake fake = new S_Fake();
                fake.PlayerId = player.Info.PlayerId;
                Broadcast(fake);
            }
        }
    }
}
