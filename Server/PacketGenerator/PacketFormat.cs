using System;
using System.Collections.Generic;
using System.Text;

namespace PacketGenerator
{
    class PacketFormat
    {
        #region Google Protobuf
        // {0} : 패킷 등록
        public static string managerFormat =
@"using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance {{ get {{ return _instance; }} }}
	#endregion

	PacketManager()
	{{
		Register();
	}}
	
	// Protocol Id, 특정 패킷으로 변경
	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	// Protocol Id, PacketHandler 특정 패킷 대상 함수
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
	
	// 모든 Protocol의 행동들을 Dic에 미리 등록하는 작업
    // 멀티쓰레드가 개입되기 전에 가장 먼저 실행 필요
	public void Register()
	{{{0}
	}}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{{
		ushort count = 0;

		// 패킷 정보 추출
		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}}

    public Action<PacketSession,IMessage,ushort> CustomHandler {{ get; set; }}

	// Packet을 만듦
	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		// Unity 클라이언트의 경우 커스텀 핸들러로 넘김
		if(CustomHandler != null)
		{{
			CustomHandler.Invoke(session, pkt, id);
		}}
		// 서버의 경우 기존 핸들러로 처리 
		else
		{{
            Action<PacketSession, IMessage> action = null;
            if (_handler.TryGetValue(id, out action))
                action.Invoke(session, pkt);
        }}
	}}

	// 특정 Packet 대상 Handler 호출
	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}}
}}";

        // {0} : MsgId
        // {1} : 패킷 이름
        public static string managerRegisterFormat =
@"		
		_onRecv.Add((ushort)MsgId.{0}, MakePacket<{1}>);
		_handler.Add((ushort)MsgId.{0}, PacketHandler.{1}Handler);";
        #endregion
        #region Personal Packet
        #region File_Packet
        // {0} : 패킷 이름/번호 목록
        // {1} : 패킷 목록
        public static string fileFormat =
@"using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

public interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}

// 패킷 분류 ID
public enum PacketID
{{
    {0}
}}

{1}";
        // {0} : 패킷 이름
        // {1} : 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";

        #endregion
        #region Packet
        // {0} : packet 이름
        // {1} : 멤버 변수들
        // {2} : 멤버 변수 Read
        // {3} : 멤버 변수 Write
        public static string packetFormat =
@"
public class {0} : IPacket
{{
    {1}

    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}    

    public void Read(ArraySegment<byte> segment)
    {{
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
        ushort count = 0;

        // size
        count += sizeof(ushort);
        // packetId
        count += sizeof(ushort);
        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ushort count = 0;
        bool success = true;

        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
        
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);

        {3}
        // size
        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }}
}}";
        // {0} : 변수 형식
        // {1} : 변수 이름
        public static string memberFormat =
@"public {0} {1};
";

        // {0} : 변수 이름
        // {1} : To~ 변수 형식
        // {2} : 변수 형식
        public static string readFormat =
@"
this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});
";

        // {0} : 변수 이름
        // {1} : 변수 형식
        public static string writeFormat =
@"
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});
";

        // {0} : 변수 이름
        public static string readStringFormat =
@"
// string
ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(s.Slice(count,{0}Len));
count += {0}Len;
";

        // {0} : 변수 이름
        public static string writeStringFormat =
@"
// string
ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;
";

        // {0} : 리스트 이름 [대문자]
        // {1} : 리스트 이름 [소문자]
        // {2} : 멤버 변수들
        // {3} : 멤버 변수 Read
        // {4} : 멤버 변수 Write
        public static string memberListFormat =
@"public class {0}
{{
    {2}
    public void Read(ReadOnlySpan<byte> s, ref ushort count)
    {{
       {3}
    }}

    public bool Write(Span<byte> s, ref ushort count)
    {{
        bool success = true;
        {4}
        return success;
    }}
}}

public List<{0}> {1}s = new List<{0}>();
";

        // {0} : 리스트 이름 [대문자]
        // {1} : 리스트 이름 [소문자]
        public static string readListFormat =
@"
// struct
{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);
    {1}s.Add({1});
}}
";

        // {0} : 리스트 이름 [대문자]
        // {1} : 리스트 이름 [소문자]
        public static string writeListFormat =
@"
// struct
ushort {1}Len = (ushort){1}s.Count;
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {1}Len);
count += sizeof(ushort);
foreach ({0} {1} in {1}s)
{{
    success &= {1}.Write(s, ref count);
}}
";

        // {0} : 변수 이름
        // {1} : 변수 형식
        public static string readByteFormat =
@"
// byte
this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});
";

        // {0} : 변수 이름
        // {1} : 변수 형식
        public static string writeByteFormat =
@"
// byte
segment.Array[segment.Offset + count] = ({1})this.{0};
count += sizeof({1});
";
        #endregion
        #endregion
    }
}
