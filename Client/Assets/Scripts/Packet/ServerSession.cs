using ServerCore;
using System;
using System.Net;
using UnityEngine;


public class ServerSession : PacketSession
{
    public override void OnConnected(EndPoint endPoint)
    {
        Debug.Log($"[Client] Connected To {endPoint}");

        // Unity Main Thread로 일감을 넘김 
        PacketManager.Instance.CustomHandler = (s, m, i) =>
        {
            PacketQueue.Instance.Push(i, m);
        };
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Debug.Log($"[Client] OnDisconnected: {endPoint}");
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnSend(int numOfBytes)
    {
        //Console.WriteLine($"[To Server] Transferred bytes : {numOfBytes}");
    }
}
