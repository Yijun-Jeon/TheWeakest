using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

class PacketHandler
{
    public static void S_ConnectServerHandler(PacketSession session, IMessage packet)
    {
        S_ConnectServer connectServerPacket = packet as S_ConnectServer;
        ServerSession serverSession = session as ServerSession;

        Managers.Network.OnConnectSuccess(connectServerPacket.IsConnected);
    }

    public static void S_InvalidNameHandler(PacketSession session, IMessage packet)
    {
        S_InvalidName invalidNamePacket = packet as S_InvalidName;
        ServerSession serverSession = session as ServerSession;

        Managers.Network.AlertMessage("유효하지 않은 이름입니다.");
    }

    public static void S_DuplicateNameHandler(PacketSession session, IMessage packet)
    {
        S_DuplicateName duplicateNamePacket = packet as S_DuplicateName;
        ServerSession serverSession = session as ServerSession;

        Managers.Network.AlertMessage("중복되는 이름입니다. 다른 이름을 입력해주세요.");
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
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
        ServerSession serverSession = session as ServerSession;

        Managers.Object.RemoveMyPlayer();
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;
        ServerSession serverSession = session as ServerSession;

        foreach (PlayerInfo player in spawnPacket.Players)
        {
            Managers.Object.Add(player, myPlayer: false);
        }
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        ServerSession serverSession = session as ServerSession;

        foreach (int id in despawnPacket.PlayerIds)
        {
            Managers.Object.Remove(id);
        }
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
    }
}
