using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System;
using System.Net;

class PacketHandler
{
    public static void C_EnterGameHandler(PacketSession session, IMessage packet)
    {
        C_EnterGame enterGamePacket = packet as C_EnterGame;
        ClientSession clientSession = session as ClientSession;

        RoomManager.Instance.Find(1).EnterGame(clientSession, enterGamePacket);
    }

    public static void C_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        C_LeaveGame leaveGamePacket = packet as C_LeaveGame;
        ClientSession clientSession = session as ClientSession;

        RoomManager.Instance.Find(1).LeaveGame(leaveGamePacket.PlayerId);
        clientSession.MyPlayer = null;
    }

    public static void C_LoadPlayerHandler(PacketSession session, IMessage packet)
    {
        C_LoadPlayer enterGamePacket = packet as C_LoadPlayer;
        ClientSession clientSession = session as ClientSession;

        RoomManager.Instance.Find(1).LoadPlayer(clientSession.MyPlayer);
    }

    public static void C_StartGameHandler(PacketSession session, IMessage packet)
    {
        C_StartGame enterGamePacket = packet as C_StartGame;
        ClientSession clientSession = session as ClientSession;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.StartGame();
    }

    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        // 멀티쓰레드 대비
        // MyPlayer가 도중에 null로 바뀌어도 무방함
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        // 멀티쓰레드 대비
        // MyPlayer가 도중에 LeaveGame 하여 GameRoom이 null로 바뀌어도 무방함
        GameRoom room = player.Room;
        if (room == null)
            return;

        room.HandleMove(player, movePacket);
    }

    public static void C_AttackHandler(PacketSession session, IMessage packet)
    {
        C_Attack attackPacket = packet as C_Attack;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.HandleAttack(player, attackPacket);
    }

    public static void C_FakeHandler(PacketSession session, IMessage packet)
    {
        C_Fake fakePacket = packet as C_Fake;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.HandleFake(player, fakePacket);
    }
}
