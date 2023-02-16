using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;

class PacketHandler
{
    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        ServerSession serverSession = session as ServerSession;
        S_Chat p = packet as S_Chat;

        //if (p.playerId == 1)
        {
            Debug.Log(p.chat);

            GameObject go = GameObject.Find("Player");
            if (go != null)
                Debug.Log("Player found");
            else
                Debug.Log("Player not found");
        }
            
    }
}
