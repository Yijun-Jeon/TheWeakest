
using ServerCore;
using System;
using System.Collections.Generic;

namespace Server.Packet
{
    public class PacketManager
    {
        #region SingleTon
        static PacketManager _instance;
        public static PacketManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PacketManager();
                return _instance;
            }
        }
        #endregion

        // Protocol Id, 특정 패킷으로 변경
        Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
        // Protocol Id, PacketHandler 특정 패킷 대상 함수
        Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

        // 모든 Protocol의 행동들을 Dic에 미리 등록하는 작업
        // 멀티쓰레드가 개입되기 전에 가장 먼저 실행 필요
        public void Register()
        {
            
            _onRecv.Add((ushort)PacketID.S_Test, MakePacket<S_Test>);
            _handler.Add((ushort)PacketID.S_Test, PacketHandler.S_TestHandler);

        }

        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
        {
            ushort count = 0;

            // 패킷 정보 추출
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += sizeof(ushort);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += sizeof(ushort);

            Action<PacketSession, ArraySegment<byte>> action = null;
            if (_onRecv.TryGetValue(id, out action))
                action.Invoke(session, buffer);
        }

        // Packet을 만들고 handler를 호출해 주는 작업
        void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
        {   
            T packet = new T();
            packet.Read(buffer);

            // PacketHandler 대상 함수 _handler에서 pakcet에 맞는 Protocol을 찾은 뒤 해당 action 추출
            Action<PacketSession, IPacket> action = null;
            if (_handler.TryGetValue(packet.Protocol, out action))
                action.Invoke(session, packet);
        }
    }
}