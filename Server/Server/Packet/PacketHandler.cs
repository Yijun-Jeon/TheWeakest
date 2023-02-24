using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System;

class PacketHandler
{
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move chatPacket = packet as C_Move;
        ClientSession serverSession = session as ClientSession;

        
    }
}
