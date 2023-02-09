using Server;
using ServerCore;
using System;

class PacketHandler
{
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;
        C_Chat p = packet as C_Chat;

        // 방에 있지 않은 상태
        if (clientSession.Room == null)
            return;

        //Console.WriteLine($"[From Client] RecvPacketId: {packet.Protocol}");
        //Console.WriteLine($"[From Client] playerId({clientSession.SessionId}) chat({p.chat})");

        GameRoom room = clientSession.Room;

        room.Push(() => { room.BroadCast(clientSession, p.chat); });
    }
}
