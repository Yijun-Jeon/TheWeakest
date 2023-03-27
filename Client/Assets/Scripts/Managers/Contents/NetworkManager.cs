using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();
    MyPlayerController _myPlayer;

    public void Init()
    {
        AlertMessage("서버 접속을 시도하는 중입니다....");

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

    public void OnConnectSuccess(bool isConnected)
    {
        if (isConnected == false)
            return;

        AlertMessage("서버 접속에 성공했습니다.");
    }

    public void AlertMessage(string message)
    {
        GameObject.FindWithTag("Alert").GetComponent<AlertListAdapter>().AddAlert(message);
    }

    public void UpdatePlayerList()
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("PlayerListPanel").
            transform.Find("PlayerList").GetComponent<PlayerListAdapter>().UpdateList();
    }

    public void UpdateKillFeed(int playerId)
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").
            transform.Find("KillList").GetComponent<KillInfoAdapter>().AddKillInfo(playerId);
    }

    public void ChangeTargetPlayer(int playerId)
    {
        PlayerController targetPlayer = Managers.Object.FindById(playerId).GetComponent<PlayerController>();
        Camera.main.GetComponent<CameraController>().SetTargetPlayer(targetPlayer);
    }
}
