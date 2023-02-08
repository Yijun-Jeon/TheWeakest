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
    C_PlayerInfoReq = 1,
	S_Test = 2,
	
}


class C_PlayerInfoReq : IPacket
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
	

    public ushort Protocol { get { return (ushort)PacketID.C_PlayerInfoReq; } }    

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
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_PlayerInfoReq);
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
class S_Test : IPacket
{
    public int testInt;
	

    public ushort Protocol { get { return (ushort)PacketID.S_Test; } }    

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
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Test);
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