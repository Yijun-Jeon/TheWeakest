using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Server
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        Map _map = new Map();

        public void Init(int mapId)
        {
            _map.LoadMap(mapId, "../../../../Common/MapData");
        }

        public void EnterGame(ClientSession session, C_EnterGame enterGamePacket)
        {
            Player myPlayer = null;
            string playerName = enterGamePacket.Name;

            // 유효하지 않은 이름 
            if (string.IsNullOrEmpty(playerName))
            {
                S_InvalidName invalidPacket = new S_InvalidName();
                session.Send(invalidPacket);
                return;
            }

            // 중복되는 이름
            var temp = _players.Where(p => p.Value.Info.Name== playerName).ToList();
            if(temp.Count != 0)
            {
                Console.WriteLine(_players.Where(x => x.Value.Info.Name == playerName).ToString());
                S_DuplicateName duplicatePacket = new S_DuplicateName();
                session.Send(duplicatePacket);
                return;
            }

            // clientSession의 MyPlayer가 이미 존재하는 경우
            if(session.MyPlayer != null)
                myPlayer = session.MyPlayer;
            else
            {
                myPlayer = PlayerManager.Instance.Add();

                myPlayer.Info.Name = enterGamePacket.Name;
                myPlayer.Info.Speed = 10.0f;
                myPlayer.Info.Power = 0;
                myPlayer.Info.PosInfo.State = PlayerState.Alive;
                myPlayer.Info.PosInfo.MoveDir = MoveDir.Idle;
                myPlayer.Info.PosInfo.PosX = 0;
                myPlayer.Info.PosInfo.PosY = 0;

                myPlayer.Session = session;
                myPlayer.Session.MyPlayer = myPlayer;
            }

            S_EnterGame enterPacket = new S_EnterGame();
            enterPacket.EnterCompleted = true;
            myPlayer.Session.Send(enterPacket);
        }

        public void LoadPlayer(Player newPlayer)
        {
            if (newPlayer == null)
                return;

            lock (_lock)
            {
                _players.Add(newPlayer.Info.PlayerId,newPlayer);
                newPlayer.Room = this;

                // 본인에게 정보 전송
                {
                    S_LoadPlayer loadPacket = new S_LoadPlayer();
                    loadPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(loadPacket);

                    // 타인들 정보
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach(Player p in _players.Values)
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
                    foreach (Player p in _players.Values)
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
                Player player = null;
                if (_players.Remove(playerId, out player) == false)
                    return;

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
                    foreach(Player p in _players.Values)
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
                foreach(Player p in _players.Values)
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
                PositionInfo movePosInfo = movePacket.PosInfo;
                PlayerInfo info = player.Info;

                // 다른 좌표로 이동하려는 경우, 갈 수 있는 곳인지 체크
                if(movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
                {
                    // 갈 수 없는 곳 
                    if (_map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                        return;
                }

                info.PosInfo.State = movePosInfo.State;
                info.PosInfo.MoveDir = movePosInfo.MoveDir;
                // 실제 이동
                _map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

                // 다른 플레이어들에게 이동을 알려줌
                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerId = player.Info.PlayerId;
                resMovePacket.PosInfo = player.Info.PosInfo;

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
