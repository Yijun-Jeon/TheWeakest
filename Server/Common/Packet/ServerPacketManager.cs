
using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager
{
    #region SingleTon
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance { get { return _instance; } }
    #endregion

    PacketManager()
    {
        Register();
    }

    // Protocol Id, 특정 패킷으로 변경
    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    // Protocol Id, PacketHandler 특정 패킷 대상 함수
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    // 모든 Protocol의 행동들을 Dic에 미리 등록하는 작업
    // 멀티쓰레드가 개입되기 전에 가장 먼저 실행 필요
    void Register()
    {
        
        _makeFunc.Add((ushort)PacketID.C_Chat, MakePacket<C_Chat>);
        _handler.Add((ushort)PacketID.C_Chat, PacketHandler.C_ChatHandler);

    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
    {
        ushort count = 0;

        // 패킷 정보 추출
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += sizeof(ushort);
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += sizeof(ushort);

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (_makeFunc.TryGetValue(id, out func))
        {
            // packet 만드는 부분
            IPacket packet = func.Invoke(session, buffer);
            if (onRecvCallback != null)
                onRecvCallback.Invoke(session, packet); // 실제로 지정한 Action 실행
            else
                HandlePacket(session, packet); // default로 정한 Handler 실행
        }  
    }

    // Packet을 만듦
    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {   
        T packet = new T();
        packet.Read(buffer);
        return packet;
    }

    // 특정 Packet 대상 Handler 호출
    public void HandlePacket(PacketSession session, IPacket packet)
    {
        // PacketHandler 대상 함수 _handler에서 packet에 맞는 Protocol을 찾은 뒤 해당 action 추출
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }
}