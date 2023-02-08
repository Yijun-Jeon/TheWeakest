using ServerCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Server.Packet
{
    class PacketHandler
    {
        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            ClientSession clientSession = session as ClientSession;
            PlayerInfoReq p = packet as PlayerInfoReq;

            Console.WriteLine($"[From Client] PlayerInfoReq : Id({p.playerId}) name({p.name})");

            foreach (PlayerInfoReq.Skill skill in p.skills)
            {
                Console.WriteLine($"[From Client] SkillInfo : Id({skill.id}) Level({skill.level}) Duration({skill.duration})");
                foreach (PlayerInfoReq.Skill.Attribute att in skill.attributes)
                    Console.WriteLine($"[From Client] Skill Attribute : att({att.att})");
            }

            Console.WriteLine($"[From Client] RecvPacketId: {packet.Protocol}");
        }

        public static void TestHandler(PacketSession session, IPacket packet)
        {

        }
    }
}
