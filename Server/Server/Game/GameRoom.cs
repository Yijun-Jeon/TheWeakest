﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Server
{
    public class GameRoom
    {
        const int MAX_PLAYER = 20;

        object _lock = new object();
        public int RoomId { get; set; }
        Random _random = new Random();

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, int> _powers = new Dictionary<int, int>();
        Map _map = new Map();

        float _attackRange = 1.5f;

        // 인게임 방 정보 
        bool _isPlaying = false;
        PlayingRoomInfo _playingRoomInfo;

        public void Init(int mapId)
        {
            _map.LoadMap(mapId, "../../../../Common/MapData");
        }

        public void StartGame()
        {
            lock (_lock)
            {
                int numOfPlayers = _players.Count;

                // 플레이어들 랜덤 파워 생성
                int[] powerArr = new int[numOfPlayers];

                for (int i = 0; i < numOfPlayers; i++)
                    powerArr[i] = i + 1;

                int random1, random2;
                int temp;

                for (int i = 0; i < numOfPlayers; i++)
                {
                    random1 = _random.Next(numOfPlayers);
                    random2 = _random.Next(numOfPlayers);

                    temp = powerArr[random1];
                    powerArr[random1] = powerArr[random2];
                    powerArr[random2] = temp;
                }

                int idx = 0;
                foreach (Player p in _players.Values)
                {
                    p.Info.Power = powerArr[idx++];
                    _powers.Add(p.Info.PlayerId, p.Info.Power);
                    // TODO : 시작 위치, 시야, 속도 조절
                }

                _playingRoomInfo = new PlayingRoomInfo()
                {
                    RoomId = RoomId,
                    PlayerCount = _players.Count,
                    AliveCount = _players.Count,
                    // TODO : 남은 시간, 꼴등 정보
                    TheWeakest = GetTheWeakest().Info,
                    AllPlayerSpeed = 6.0f,
                    RemainTime = 90.0f
                };

                SetAllPlayerSpeed();

                Player theWeakest = GetTheWeakest();
                if (theWeakest == null)
                    return;
                _playingRoomInfo.TheWeakest = GetTheWeakest().Info;

                // 게임 시작 모두에게 알림
                S_StartGame startGamePacket = new S_StartGame();
                foreach (Player p in _players.Values)
                {
                    startGamePacket.Players.Add(p.Info);
                }
                startGamePacket.RoomInfo = _playingRoomInfo;
                Broadcast(startGamePacket);

                _isPlaying = true;
                JobTimer.Instance.Push(HandleTIme, 1000);
            }

        }

        public void EnterGame(ClientSession session, C_EnterGame enterGamePacket)
        {
            Player myPlayer = null;
            string playerName = enterGamePacket.Name;

            if (_isPlaying)
                return;

            if (_players.Count == MAX_PLAYER)
                return;

            // 유효하지 않은 이름 
            if (string.IsNullOrEmpty(playerName))
            {
                S_InvalidName invalidPacket = new S_InvalidName();
                session.Send(invalidPacket);
                return;
            }

            // 중복되는 이름
            var temp = _players.Where(p => p.Value.Info.Name == playerName).ToList();
            if (temp.Count != 0)
            {
                Console.WriteLine(_players.Where(x => x.Value.Info.Name == playerName).ToString());
                S_DuplicateName duplicatePacket = new S_DuplicateName();
                session.Send(duplicatePacket);
                return;
            }

            // clientSession의 MyPlayer가 이미 존재하는 경우
            if (session.MyPlayer != null)
                myPlayer = session.MyPlayer;
            else
            {
                myPlayer = PlayerManager.Instance.Add();

                myPlayer.Info.Name = enterGamePacket.Name;
                myPlayer.Info.Speed = 6.0f;
                myPlayer.Info.Power = 0;
                myPlayer.Info.KillCount = 0;
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
                _players.Add(newPlayer.Info.PlayerId, newPlayer);
                newPlayer.Room = this;

                // 본인에게 정보 전송
                {
                    S_LoadPlayer loadPacket = new S_LoadPlayer();
                    loadPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(loadPacket);

                    // 타인들 정보
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (newPlayer != p)
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
                        if (newPlayer != p)
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

                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            p.Session.Send(despawnPacket);
                    }
                }

                // 게임 도중 떠날 경우 
                if (_isPlaying)
                {
                    // 이미 죽은 플레이어가 나갈 경우에는 인게임 지장 X
                    if (player.Info.PosInfo.State != PlayerState.Dead)
                    {
                        _powers.Remove(playerId);
                        _playingRoomInfo.AliveCount -= 1;

                        SetAllPlayerSpeed();

                        Player theWeakest = GetTheWeakest();
                        if (theWeakest == null)
                            return;
                        _playingRoomInfo.TheWeakest = GetTheWeakest().Info;

                        S_PlayingRoomInfoChange roomInfoChange = new S_PlayingRoomInfoChange();
                        roomInfoChange.RoomInfo = _playingRoomInfo;
                        Broadcast(roomInfoChange);
                    }
                }
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players.Values)
                {
                    p.Session.Send(packet);
                }
            }
        }

        // 플레이어 이동 처리
        public void HandleMove(Player player, C_Move movePacket)
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
                if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
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

                // 공격 패킷 전송 
                S_Attack attack = new S_Attack();
                attack.PlayerId = player.Info.PlayerId;
                Broadcast(attack);

                // 공격 범위 내 상대방이 있는지 체크
                foreach (PlayerInfo p in attackPacket.Enemys)
                {
                    if (GetDistance(info.PosInfo, p.PosInfo) <= _attackRange)
                    {
                        Player enemy = null;
                        if (_players.TryGetValue(p.PlayerId, out enemy) == false)
                            return;

                        // 상대방이 이미 죽은 상태라면 패스
                        if (enemy.Info.PosInfo.State == PlayerState.Dead)
                            continue;

                        HandleDead(player, enemy);
                    }
                }
            }
        }

        // 플레이어 사망 처리
        public void HandleDead(Player player, Player enemy)
        {
            // 공격자의 공격력이 더 높음
            if (player.Info.Power > enemy.Info.Power)
            {
                // 사망 처리
                enemy.Info.PosInfo.State = PlayerState.Dead;
                _powers.Remove(enemy.Info.PlayerId);
                // 킬러 플레이어 킬카운트 증가
                player.Info.KillCount += 1;

                S_Dead deadPacket = new S_Dead();
                deadPacket.KillerPlayer = player.Info;
                deadPacket.KilledPlayer = enemy.Info;

                Broadcast(deadPacket);
            }
            // 공격자의 공격력이 더 낮음
            else if (player.Info.Power < enemy.Info.Power)
            {
                // 사망 처리
                player.Info.PosInfo.State = PlayerState.Dead;
                _powers.Remove(player.Info.PlayerId);
                // 킬러 플레이어 킬카운트 증가
                enemy.Info.KillCount += 1;

                S_Dead deadPacket = new S_Dead();
                deadPacket.KillerPlayer = enemy.Info;
                deadPacket.KilledPlayer = player.Info;

                Broadcast(deadPacket);
            }
            // lobby 에서의 공격 
            else
            {
                return;
            }

            _playingRoomInfo.AliveCount -= 1;
            SetAllPlayerSpeed();

            Player theWeakest = GetTheWeakest();
            if (theWeakest == null)
                return;
            _playingRoomInfo.TheWeakest = GetTheWeakest().Info;

            S_PlayingRoomInfoChange roomInfoChange = new S_PlayingRoomInfoChange();
            roomInfoChange.RoomInfo = _playingRoomInfo;
            Broadcast(roomInfoChange);

            // TODO : 남은 플레이어들 스탯, 시야 조정
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

                // 죽은 척 패킷 전송
                S_Fake fake = new S_Fake();
                fake.PlayerId = player.Info.PlayerId;
                Broadcast(fake);
            }
        }

        // 카메라 관전모드 처리
        public void HandleWatch(Player player, C_WatchOther watchPacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                Player targetPlayer = null;
                if (_players.TryGetValue(watchPacket.TargetId, out targetPlayer) == false)
                    return;

                PlayerInfo info = player.Info;

                // 관전을 할 수 있는 상태 
                if (_isPlaying == false || info.PosInfo.State == PlayerState.Dead)
                {
                    S_WatchOther watchOther = new S_WatchOther();
                    watchOther.TargetId = targetPlayer.Info.PlayerId;
                    player.Session.Send(watchOther);
                }
            }
        }

        // 종료시간 타이머
        public void HandleTIme()
        {
            if (_isPlaying == false && _playingRoomInfo.AliveCount <= 1)
                return;

            S_PlayingRoomInfoChange roomInfoChange = new S_PlayingRoomInfoChange();
            _playingRoomInfo.RemainTime -= 1.0f;
            roomInfoChange.RoomInfo = _playingRoomInfo;
            Broadcast(roomInfoChange);

            // 1초마다 전송
            JobTimer.Instance.Push(HandleTIme, 1000);
        }

        float GetDistance(PositionInfo myPos, PositionInfo enemyPos)
        {
            return (float)Math.Sqrt(Math.Pow(enemyPos.PosX - myPos.PosX, 2) + Math.Pow(enemyPos.PosY - myPos.PosY, 2));
        }

        Player GetTheWeakest()
        {
            lock (_lock)
            {
                List<int> powers = _powers.Values.ToList();
                if (powers.Count == 0)
                    return null;

                int minPower = powers.Min();

                // 현재 꼴등 플레이어 
                Player weakest = _players.Where(p => p.Value.Info.Power == minPower).First().Value;
                return weakest;
            }
        }

        void SetAllPlayerSpeed()
        {
            lock (_lock)
            {
                if (_playingRoomInfo.TheWeakest == null)
                    return;

                _playingRoomInfo.AllPlayerSpeed = 10f - _playingRoomInfo.AliveCount * 0.3f;
                // 꼴등 이속 버프 
                float weakestBuff = Math.Max(0, _playingRoomInfo.AliveCount - 2) * 0.2f;
                foreach (Player p in _players.Values)
                {
                    p.Info.Speed = _playingRoomInfo.AllPlayerSpeed;
                    if (p.Info.PlayerId == _playingRoomInfo.TheWeakest.PlayerId)
                    {
                        p.Info.Speed += weakestBuff;
                        _playingRoomInfo.TheWeakest.Speed = p.Info.Speed;
                    }
                }
            }
        }
    }
}
