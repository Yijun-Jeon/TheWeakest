using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;

class PacketHandler
{
    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
    {
        ServerSession serverSession = session as ServerSession;
        S_BroadcastEnterGame enterPacket = packet as S_BroadcastEnterGame;

        PlayerManager.Instance.EnterGame(enterPacket);
    }

    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
        ServerSession serverSession = session as ServerSession;
        S_BroadcastLeaveGame leavePacket = packet as S_BroadcastLeaveGame;

        PlayerManager.Instance.LeaveGame(leavePacket);
    }

    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        ServerSession serverSession = session as ServerSession;
        S_BroadcastMove movePacket = packet as S_BroadcastMove;

        PlayerManager.Instance.Move(movePacket);
    }

    public static void S_PlayerListHandler(PacketSession session, IPacket packet)
    {
        ServerSession serverSession = session as ServerSession;
        S_PlayerList playerList = packet as S_PlayerList;

        PlayerManager.Instance.Add(playerList);
    }
}
