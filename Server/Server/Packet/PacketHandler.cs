using Server;
using ServerCore;
using System;

class PacketHandler
{
    public static void C_LeaveGameHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;
        C_LeaveGame p = packet as C_LeaveGame;

        // 방에 있지 않은 상태
        if (clientSession.Room == null)
            return;

        GameRoom room = clientSession.Room;
        room.Push(() => { room.Leave(clientSession); });
    }

    public static void C_MoveHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;
        C_Move p = packet as C_Move;

        // 방에 있지 않은 상태
        if (clientSession.Room == null)
            return;

        //Console.WriteLine($"{p.posX}, {p.posY}, {p.posZ}");

        GameRoom room = clientSession.Room;
        room.Push(() => { room.Move(clientSession, p); });
    }
}
