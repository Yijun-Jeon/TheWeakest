using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PacketGenerator
{
    class PacketFormat
    {
        // {0} : packet 이름
        // {1} : 멤버 변수들
        // {2} : 멤버 변수 Read
        // {3} : 멤버 변수 Write
        public static string packetFormat =
@"
class {0}
{{
    {1}

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
count += sizeof({2});";

        // {0} : 변수 이름
        // {1} : 변수 형식
        public static string writeFormat =
@"
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});";

        // {0} : 변수 이름
        public static string readStringFormat =
@"
// string
ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(segment.Array, count, {0}Len);
count += {0}Len;";

        // {0} : 변수 이름
        public static string writeStringFormat =
@"
// string
ushort {0}Len = (ushort)Encoding.Unicode.GetByteCount(this.{0});
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
Array.Copy(Encoding.Unicode.GetBytes(this.{0}), 0, segment.Array, count, {0}Len);
count += {0}Len;";
    }
}
