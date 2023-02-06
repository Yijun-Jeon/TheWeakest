using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace DummyClient
{
    // Packet format
    public abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    // TEST Packet
    class PlayerInfoReq : Packet
    {
        public long playerId;

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> s)
        {
            ushort count = 0;

            // 패킷 데이터 추출
            this.size = BitConverter.ToUInt16(new ReadOnlySpan<byte>(s.Array, s.Offset + count,s.Count - count));
            count += sizeof(ushort);
            this.packetId = BitConverter.ToUInt16(new ReadOnlySpan<byte>(s.Array, s.Offset + count,s.Count - count));
            count += sizeof(ushort);

            this.playerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(s.Array,s.Offset+count,s.Count - count));
            count += sizeof(long);
        }

        public override ArraySegment<byte> Write()
        {
            ushort count = 0;
            bool success = true;

            ArraySegment<byte> s = SendBufferHelper.Open(4096);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.packetId);
            count += sizeof(ushort);

            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.playerId);
            count += sizeof(long);

            // size
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }

    // 패킷 분류 ID
    public enum PacketID
    {
        PlayerInfoReq = 1,
    }

    class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Client] Connected To {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 10000 }; ;

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
