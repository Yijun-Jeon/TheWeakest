using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Server] OnConnected: {endPoint}");

            Thread.Sleep(100);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Server] OnDisconnected: {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;

            // 패킷 데이터 추출
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += sizeof(ushort);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += sizeof(ushort);

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);
                        Console.WriteLine($"[From Client] PlayerInfoReq : Id({p.playerId}) name({p.name})");

                        foreach(PlayerInfoReq.Skill skill in p.skills)
                        {
                            Console.WriteLine($"[From Client] SkillInfo : Id({skill.id}) Level({skill.level}) Duration({skill.duration})");
                            foreach (PlayerInfoReq.Skill.Attribute att in skill.attributes)
                                Console.WriteLine($"[From Client] Skill Attribute : att({att.att})");
                        }
                    }
                    break;
            }

            Console.WriteLine($"[From Client] RecvPacketId: {id}, Size: {size}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"[To Client] Transferred bytes : {numOfBytes}");
        }
    }
}
