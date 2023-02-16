using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();

    void Start()
    {
        // DNS
        /*string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[1];*/

        // 서버 공용 IP 주소
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("61.102.132.68"), 7000);

        Connector connector = new Connector();
        connector.Connect(endPoint, () => { return _session; }, 1);
    }

    public void Send(ArraySegment<byte> sendBuff)
    {
        _session.Send(sendBuff);
    }

    void Update()
    {
        IPacket packet = PacketQueue.Instance.Pop();
        if(packet != null)
        {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }
}
