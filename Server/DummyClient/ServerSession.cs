using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace DummyClient
{
    class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Client] Connected To {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 10000, name="Yijun" };
            var skill = new PlayerInfoReq.Skill() { id = 101, level = 1, duration = 3.0f };
            skill.attributes.Add(new PlayerInfoReq.Skill.Attribute() { att = 777 });
            packet.skills.Add(skill);
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 201, level = 2, duration = 4.0f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 301, level = 3, duration = 5.0f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 401, level = 4, duration = 6.0f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 501, level = 5, duration = 7.0f });

            ArraySegment<byte> sendBuff = packet.Write();
            if(sendBuff != null)
                Send(sendBuff);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Client] OnDisconnected: {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            // 패킷 데이터 추출
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + sizeof(ushort));

            Console.WriteLine($"[From Server] packetId({packetId}) size({size})");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"[To Server] Transferred bytes : {numOfBytes}");
        }
    }
}
