using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System;

class PacketHandler
{
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

        room.HandleMove(player,movePacket);
    }
}
