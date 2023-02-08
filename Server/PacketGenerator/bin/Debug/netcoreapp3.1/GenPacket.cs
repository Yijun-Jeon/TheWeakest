
class PlayerInfoReq
{
    public long playeId;
	public string name;
	public struct Skill
	{
	    public int id;
		public short level;
		public float duration;
		
	    public void Read(ReadOnlySpan<byte> s, ref ushort count)
	    {
	       
			this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
			count += sizeof(short);
			this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
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
        
		this.playeId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
		
		// string
		ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.name = Encoding.Unicode.GetString(segment.Array, count, nameLen);
		count += nameLen;
		
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
        
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playeId);
		count += sizeof(long);
		
		// string
		ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
		count += sizeof(ushort);
		Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
		count += nameLen;
		
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