using ServerCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Server
{
    class PacketHandler
    {
        public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            ClientSession clientSession = session as ClientSession;
            C_PlayerInfoReq p = packet as C_PlayerInfoReq;

            Console.WriteLine($"[From Client] PlayerInfoReq : Id({p.playerId}) name({p.name})");

            foreach (C_PlayerInfoReq.Skill skill in p.skills)
            {
                Console.WriteLine($"[From Client] SkillInfo : Id({skill.id}) Level({skill.level}) Duration({skill.duration})");
                foreach (C_PlayerInfoReq.Skill.Attribute att in skill.attributes)
                    Console.WriteLine($"[From Client] Skill Attribute : att({att.att})");
            }

            Console.WriteLine($"[From Client] RecvPacketId: {packet.Protocol}");
        }
    }
}
