using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace DummyClient
{
    // 패킷 분류 ID
    public enum PacketID
    {
        PlayerInfoReq = 1,
        Test = 2,

    }


    class PlayerInfoReq
    {
        public long playerId;
        public string name;
        public byte testByte;
        public class Skill
        {
            public int id;
            public short level;
            public float duration;
            public class Attribute
            {
                public int att;

                public void Read(ReadOnlySpan<byte> s, ref ushort count)
                {

                    this.att = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                    count += sizeof(int);

                }

                public bool Write(Span<byte> s, ref ushort count)
                {
                    bool success = true;

                    success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.att);
                    count += sizeof(int);

                    return success;
                }
            }

            public List<Attribute> attributes = new List<Attribute>();

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {

                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);

                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);

                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);

                // struct
                attributes.Clear();
                ushort attributeLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
                count += sizeof(ushort);
                for (int i = 0; i < attributeLen; i++)
                {
                    Attribute attribute = new Attribute();
                    attribute.Read(s, ref count);
                    attributes.Add(attribute);
                }

            }

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;

                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                count += sizeof(int);

                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                count += sizeof(short);

                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                count += sizeof(float);

                // struct
                ushort attributeLen = (ushort)attributes.Count;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), attributeLen);
                count += sizeof(ushort);
                foreach (Attribute attribute in attributes)
                {
                    success &= attribute.Write(s, ref count);
                }

                return success;
            }
        }

        public List<Skill> skills = new List<Skill>();


        public void Read(ArraySegment<byte> segment)
        {
            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            ushort count = 0;

            // size
            count += sizeof(ushort);
            // packetId
            count += sizeof(ushort);

            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            // string
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(segment.Array, count, nameLen);
            count += nameLen;

            // byte
            this.testByte = (byte)segment.Array[segment.Offset + count];
            count += sizeof(byte);

            // struct
            skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            for (int i = 0; i < skillLen; i++)
            {
                Skill skill = new Skill();
                skill.Read(s, ref count);
                skills.Add(skill);
            }

        }

        public ArraySegment<byte> Write()
        {
            ushort count = 0;
            bool success = true;

            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
            count += sizeof(ushort);


            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);

            // string
            ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
            count += nameLen;

            // byte
            segment.Array[segment.Offset + count] = (byte)this.testByte;
            count += sizeof(byte);

            // struct
            ushort skillLen = (ushort)skills.Count;
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), skillLen);
            count += sizeof(ushort);
            foreach (Skill skill in skills)
            {
                success &= skill.Write(s, ref count);
            }

            // size
            success &= BitConverter.TryWriteBytes(s, count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }
    class Test
    {
        public int testInt;


        public void Read(ArraySegment<byte> segment)
        {
            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            ushort count = 0;

            // size
            count += sizeof(ushort);
            // packetId
            count += sizeof(ushort);

            this.testInt = BitConverter.ToInt32(s.Slice(count, s.Length - count));
            count += sizeof(int);

        }

        public ArraySegment<byte> Write()
        {
            ushort count = 0;
            bool success = true;

            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.Test);
            count += sizeof(ushort);


            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.testInt);
            count += sizeof(int);

            // size
            success &= BitConverter.TryWriteBytes(s, count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }

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
