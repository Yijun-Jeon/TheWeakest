using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();
    MyPlayerController _myPlayer;

    Coroutine _coGoToLogin;

    public void Init()
    {
        Managers.UI.AlertMessage("서버 접속을 시도하는 중입니다....");

        // DNS
        //string host = Dns.GetHostName();
        //IPHostEntry ipHost = Dns.GetHostEntry(host);
        //IPAddress ipAddr = ipHost.AddressList[1];

        // 서버 공용 IP 주소 
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("61.102.132.68"), 7000);

        Connector connector = new Connector();
        connector.Connect(endPoint, () => { return _session; }, 1);
    }

    public void Send(IMessage packet)
    {
        _session.Send(packet);
    }

    public void Update()
    {
        List<PacketMessage> list = PacketQueue.Instance.PopAll();
        foreach (PacketMessage packet in list)
        {
            Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(_session, packet.Message);
        }
    }
}
