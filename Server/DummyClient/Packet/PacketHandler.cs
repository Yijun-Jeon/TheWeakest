using DummyClient;
using ServerCore;

class PacketHandler
{
    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
    {
        ServerSession serverSession = session as ServerSession;
        S_BroadcastEnterGame p = packet as S_BroadcastEnterGame;
    }

    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
        ServerSession serverSession = session as ServerSession;
        S_BroadcastLeaveGame p = packet as S_BroadcastLeaveGame;
    }

    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        ServerSession serverSession = session as ServerSession;
        S_BroadcastMove p = packet as S_BroadcastMove;
    }

    public static void S_PlayerListHandler(PacketSession session, IPacket packet)
    {
        ServerSession serverSession = session as ServerSession;
        S_PlayerList p = packet as S_PlayerList;
    }
}
