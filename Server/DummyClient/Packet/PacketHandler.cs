using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

class PacketHandler
{
    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        ServerSession serverSession = session as ServerSession;
        S_Chat p = packet as S_Chat;

        //Console.WriteLine($"[From Server] RecvPacketId: {packet.Protocol}");
        Console.WriteLine($"[From Server] playerId({p.playerId}) chat({p.chat})");
    }
}
