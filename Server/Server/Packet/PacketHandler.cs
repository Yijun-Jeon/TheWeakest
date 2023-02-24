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

        if (clientSession.MyPlayer == null)
            return;

        if (clientSession.MyPlayer.Room == null)
            return;

        // TODO : 검증

        // 일단 서버에서 좌표 이동
        PlayerInfo info = clientSession.MyPlayer.Info;
        info.PosInfo = movePacket.PosInfo;

        // 다른 플레이어들에게 이동을 알려줌
        S_Move resMovePacket = new S_Move();
        resMovePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
        resMovePacket.PosInfo = movePacket.PosInfo;

        clientSession.MyPlayer.Room.Broadcast(resMovePacket);
    }
}
