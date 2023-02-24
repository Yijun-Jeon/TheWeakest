﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
    public class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public Player MyPlayer { get; set; }

        public void Send(IMessage packet)
        {
            // MsgId 추출
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)size + 4), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            Send(new ArraySegment<byte>(sendBuffer));
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Server] OnConnected: {endPoint}");

            // PROTO TEST
            MyPlayer = PlayerManager.Instance.Add();
            {
                MyPlayer.Info.Name = $"Player_{MyPlayer.Info.PlayerId}";
                MyPlayer.Info.PosInfo.State = PlayerState.Alive;
                MyPlayer.Info.PosInfo.MoveDir = MoveDir.Idle;
                MyPlayer.Info.PosInfo.PosX = 0;
                MyPlayer.Info.PosInfo.PosY = 0;

                MyPlayer.Session = this;
            }

            // 1번방에 플레이어 입장
            RoomManager.Instance.Find(1).EnterGame(MyPlayer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            RoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.PlayerId);

            SessionManager.Instance.Remove(this);

            Console.WriteLine($"[Server] OnDisconnected: {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
           PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"[To Client] Transferred bytes : {numOfBytes}");
        }
    }
}
