using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

class PacketHandler
{
    public static void S_ConnectServerHandler(PacketSession session, IMessage packet)
    {
        S_ConnectServer connectServerPacket = packet as S_ConnectServer;
        ServerSession serverSession = session as ServerSession;

        Managers.UI.OnConnectSuccess(connectServerPacket.IsConnected);
    }

    public static void S_InvalidNameHandler(PacketSession session, IMessage packet)
    {
        S_InvalidName invalidNamePacket = packet as S_InvalidName;
        ServerSession serverSession = session as ServerSession;

        Managers.UI.AlertMessage("유효하지 않은 이름입니다.");
    }

    public static void S_DuplicateNameHandler(PacketSession session, IMessage packet)
    {
        S_DuplicateName duplicateNamePacket = packet as S_DuplicateName;
        ServerSession serverSession = session as ServerSession;

        Managers.UI.AlertMessage("중복되는 이름입니다. 다른 이름을 입력해주세요.");
    }

    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
        ServerSession serverSession = session as ServerSession;

        SceneManager.LoadScene("GameScene");

        C_LoadPlayer loadPacket = new C_LoadPlayer();
        Managers.Network.Send(loadPacket);
    }

    public static void S_LoadPlayerHandler(PacketSession session, IMessage packet)
    {
        S_LoadPlayer loadPacket = packet as S_LoadPlayer;
        ServerSession serverSession = session as ServerSession;

        Managers.Object.Add(loadPacket.Player, myPlayer: true);
        Managers.UI.UpdatePlayerList();
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
        ServerSession serverSession = session as ServerSession;

        Managers.Object.MyPlayer.GoToLogin(0f);
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;
        ServerSession serverSession = session as ServerSession;

        foreach (PlayerInfo player in spawnPacket.Players)
        {
            Managers.Object.Add(player, myPlayer: false);
        }
        Managers.UI.UpdatePlayerList();
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        ServerSession serverSession = session as ServerSession;

        foreach (int id in despawnPacket.PlayerIds)
        {
            Managers.Object.Remove(id);
        }
        Managers.UI.UpdatePlayerList();
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;
        ServerSession serverSession = session as ServerSession;

        // 이동 해당 플레이어 검색
        GameObject go = Managers.Object.FindById(movePacket.PlayerId);
        if (go == null)
            return;

        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;

        pc.PosInfo = movePacket.PosInfo;
    }

    public static void S_AttackHandler(PacketSession session, IMessage packet)
    {
        S_Attack attackPacket = packet as S_Attack;
        ServerSession serverSession = session as ServerSession;

        GameObject go = Managers.Object.FindById(attackPacket.PlayerId);
        if (go == null)
            return;

        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.Attack();
        }
    }

    public static void S_FakeHandler(PacketSession session, IMessage packet)
    {
        S_Fake fakePacket = packet as S_Fake;
        ServerSession serverSession = session as ServerSession;

        GameObject go = Managers.Object.FindById(fakePacket.PlayerId);
        if (go == null)
            return;

        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.Fake();
        }
    }

    public static void S_StartGameHandler(PacketSession session, IMessage packet)
    {
        S_StartGame startGamePacket = packet as S_StartGame;
        ServerSession serverSession = session as ServerSession;

        foreach (PlayerInfo player in startGamePacket.Players)
        {
            GameObject go = Managers.Object.FindById(player.PlayerId);
            PlayerController pc = go.GetComponent<PlayerController>();
            pc.PosInfo = player.PosInfo;
            pc.Power = player.Power;
            pc.Speed = player.Speed;
        }

        Managers.UI.ActiveStartBtn(false);
        Managers.UI.ActiveCancelBtn(false);
        Managers.UI.ActivePlayerListPanel(false);
        Managers.UI.ActiveInGamePanel(true);
        Managers.UI.UpdateRemainText(startGamePacket.RoomInfo.PlayerCount, startGamePacket.RoomInfo.AliveCount);

        GameObject weakestObject = Managers.Object.FindById(startGamePacket.RoomInfo.TheWeakest.PlayerId);
        if (weakestObject == null)
            return;

        PlayerController theWeakest = weakestObject.GetComponent<PlayerController>();
        if (theWeakest == null)
            return;

        Managers.Object.SetTheWeakest(theWeakest);
        Managers.UI.UpdateRemainText(startGamePacket.RoomInfo.PlayerCount, startGamePacket.RoomInfo.AliveCount);
    }

    public static void S_DeadHandler(PacketSession session, IMessage packet)
    {
        S_Dead deadPacket = packet as S_Dead;
        ServerSession serverSession = session as ServerSession;

        // 사망 플레이어 
        GameObject killed = Managers.Object.FindById(deadPacket.KilledPlayer.PlayerId);
        if (killed == null)
            return;

        // 킬한 플레이어
        GameObject killer = Managers.Object.FindById(deadPacket.KillerPlayer.PlayerId);
        if (killer == null)
            return;

        // 사망 플레이어가 내 플레이어 
        if (deadPacket.KilledPlayer.PlayerId == Managers.Object.MyPlayer.Id)
        {
            MyPlayerController mp = killed.GetComponent<MyPlayerController>();
            if (mp != null)
            {
                mp.Killed();
            }
            Managers.UI.ActivePlayerListPanel(true);
        }
        // 사망 플레이어가 다른 플레이어 
        else
        {
            PlayerController pc = killed.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.Killed();
            }
        }
        // 사망 플레이어 파워 공개 
        killed.transform.Find("Canvas").transform.Find("PowerText").gameObject.SetActive(true);
        // 킬로그 추가
        Managers.UI.UpdateKillFeed(deadPacket.KilledPlayer.PlayerId);

        // 킬한 내 플레이어 킬 카운트 증가 
        if (deadPacket.KillerPlayer.PlayerId == Managers.Object.MyPlayer.Id)
        {
            MyPlayerController mp = killer.GetComponent<MyPlayerController>();
            if (mp != null)
            {
                mp.Kill(deadPacket.KillerPlayer.KillCount);
            }
        }
    }

    public static void S_WatchOtherHandler(PacketSession session, IMessage packet)
    {
        S_WatchOther watchOther = packet as S_WatchOther;
        ServerSession serverSession = session as ServerSession;

        GameObject go = Managers.Object.FindById(watchOther.TargetId);
        if (go == null)
            return;

        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;

        Managers.UI.UpdateTargetPlayer(watchOther.TargetId);
    }

    public static void S_PlayingRoomInfoChangeHandler(PacketSession session, IMessage packet)
    {
        S_PlayingRoomInfoChange roomInfoPacket = packet as S_PlayingRoomInfoChange;
        ServerSession serverSession = session as ServerSession;

        GameObject go = Managers.Object.FindById(roomInfoPacket.RoomInfo.TheWeakest.PlayerId);
        if (go == null)
            return;

        PlayerController theWeakest = go.GetComponent<PlayerController>();
        if (theWeakest == null)
            return;

        theWeakest.Speed = roomInfoPacket.RoomInfo.TheWeakest.Speed;

        Managers.Object.SetTheWeakest(theWeakest);
        Managers.UI.UpdateRemainText(roomInfoPacket.RoomInfo.PlayerCount, roomInfoPacket.RoomInfo.AliveCount);
        // 타이머 조정
        Managers.UI.UpdateTime(roomInfoPacket.RoomInfo.RemainTime);

        // 플레이어 속도 조정 
        Managers.Object.SetAllPlayerSpeed(roomInfoPacket.RoomInfo);

    }

    public static void S_EndGameHandler(PacketSession session, IMessage packet)
    {
        S_EndGame endPacket = packet as S_EndGame;
        ServerSession serverSession = session as ServerSession;

        GameObject go = Managers.Object.FindById(endPacket.WinnerId);
        if (go == null)
            return;

        PlayerController winner = go.GetComponent<PlayerController>();
        if (winner == null)
            return;

        Managers.UI.ActiveEndGamePanel(winner.Id);
        Managers.Object.MyPlayer.GoToLogin(15.0f);
    }
}
