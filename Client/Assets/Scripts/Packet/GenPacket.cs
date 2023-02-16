using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}

// 패킷 분류 ID
public enum PacketID
{
    C_Chat = 1,
	S_Chat = 2,
	
}


public class C_Chat : IPacket
{
    public string chat;
	

    public ushort Protocol { get { return (ushort)PacketID.C_Chat; } }    

    public void Read(ArraySegment<byte> segment)
    {
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
        ushort count = 0;

        // size
        count += sizeof(ushort);
        // packetId
        count += sizeof(ushort);
        
		// string
		ushort chatLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(s.Slice(count,chatLen));
		count += chatLen;
		
    }

    public ArraySegment<byte> Write()
    {
        ushort count = 0;
        bool success = true;

        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
        
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_Chat);
        count += sizeof(ushort);

        
		// string
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;
		
        // size
        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }
}
public class S_Chat : IPacket
{
    public int playerId;
	public string chat;
	

    public ushort Protocol { get { return (ushort)PacketID.S_Chat; } }    

    public void Read(ArraySegment<byte> segment)
    {
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
        ushort count = 0;

        // size
        count += sizeof(ushort);
        // packetId
        count += sizeof(ushort);
        
		this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		
		// string
		ushort chatLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(s.Slice(count,chatLen));
		count += chatLen;
		
    }

    public ArraySegment<byte> Write()
    {
        ushort count = 0;
        bool success = true;

        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
        
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Chat);
        count += sizeof(ushort);

        
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int);
		
		// string
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;
		
        // size
        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }
}