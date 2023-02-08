using System;
using System.IO;
using System.Xml;

namespace PacketGenerator
{
    internal class Program
    {
        // output 파일
        static string genPackets;

        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using (XmlReader r = XmlReader.Create("../../../PDL.xml", settings))
            {
                // 내용 부분으로 이동
                r.MoveToContent();

                while (r.Read())
                {
                    // 패킷 태그의 시작부분을 만남
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                }

                File.WriteAllText("GenPacket.cs", genPackets);
            }
        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)
                return;

            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invaild packet node");
                return;
            }

            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }

            // 정상적인 패킷이므로 파싱
            Tuple<string,string,string> t = ParseMembers(r);

            // {0} : packet 이름, {1} : 멤버 변수들, {2} : 멤버 변수 Read, {3} : 멤버 변수 Write
            genPackets += string.Format(PacketFormat.packetFormat,
                packetName,t.Item1, t.Item2,t.Item3);
        }

        public static Tuple<string, string, string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            // 파싱 대상들의 depth
            int depth = r.Depth + 1;
            while (r.Read())
            {
                // 패킷의 끝 부분 만남
                if (r.Depth != depth)
                    break;

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        // {0} : 변수 형식, {1} : 변수 이름
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        // {0} : 변수 이름, {1} : To~ 변수 형식, {2} : 변수 형식
                        readCode += string.Format(PacketFormat.readFormat, memberName,ToMemberType(memberType), memberType);
                        // {0} : 변수 이름, {1} : 변수 형식
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        // {0} : 변수 형식, {1} : 변수 이름
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        // {0} : 변수 이름
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        // {0} : 변수 이름
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(r);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;
                    case "byte":
                    case "sbyte":
                    default:
                        break;
                }

                
            }
            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUint16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static Tuple<string,string,string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            Tuple<string, string, string> t = ParseMembers(r);
            // {0} : 리스트 이름 [대문자], {1} : 리스트 이름 [소문자], {2} : 멤버 변수들, {3} : 멤버 변수 Read, {4} : 멤버 변수 Write
            memberCode += string.Format(PacketFormat.memberListFormat,
                FirstCharToUpper(listName),FirstCharToLower(listName),t.Item1,t.Item2,t.Item3);

            // {0} : 리스트 이름 [대문자], {1} : 리스트 이름 [소문자]
            readCode += string.Format(PacketFormat.readListFormat, FirstCharToUpper(listName), FirstCharToLower(listName));

            // {0} : 리스트 이름 [대문자], {1} : 리스트 이름 [소문자]
            writeCode += string.Format(PacketFormat.writeListFormat, FirstCharToUpper(listName), FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        // 첫 글자 대문자로 변환
        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        // 첫 글자 소문자로 변환
        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}
