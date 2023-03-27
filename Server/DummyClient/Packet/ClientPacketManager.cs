using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}
	
	// Protocol Id, 특정 패킷으로 변경
	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	// Protocol Id, PacketHandler 특정 패킷 대상 함수
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
	
	// 모든 Protocol의 행동들을 Dic에 미리 등록하는 작업
    // 멀티쓰레드가 개입되기 전에 가장 먼저 실행 필요
	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.SConnectServer, MakePacket<S_ConnectServer>);
		_handler.Add((ushort)MsgId.SConnectServer, PacketHandler.S_ConnectServerHandler);		
		_onRecv.Add((ushort)MsgId.SEnterGame, MakePacket<S_EnterGame>);
		_handler.Add((ushort)MsgId.SEnterGame, PacketHandler.S_EnterGameHandler);		
		_onRecv.Add((ushort)MsgId.SLeaveGame, MakePacket<S_LeaveGame>);
		_handler.Add((ushort)MsgId.SLeaveGame, PacketHandler.S_LeaveGameHandler);		
		_onRecv.Add((ushort)MsgId.SSpawn, MakePacket<S_Spawn>);
		_handler.Add((ushort)MsgId.SSpawn, PacketHandler.S_SpawnHandler);		
		_onRecv.Add((ushort)MsgId.SDespawn, MakePacket<S_Despawn>);
		_handler.Add((ushort)MsgId.SDespawn, PacketHandler.S_DespawnHandler);		
		_onRecv.Add((ushort)MsgId.SMove, MakePacket<S_Move>);
		_handler.Add((ushort)MsgId.SMove, PacketHandler.S_MoveHandler);		
		_onRecv.Add((ushort)MsgId.SAttack, MakePacket<S_Attack>);
		_handler.Add((ushort)MsgId.SAttack, PacketHandler.S_AttackHandler);		
		_onRecv.Add((ushort)MsgId.SFake, MakePacket<S_Fake>);
		_handler.Add((ushort)MsgId.SFake, PacketHandler.S_FakeHandler);		
		_onRecv.Add((ushort)MsgId.SStartGame, MakePacket<S_StartGame>);
		_handler.Add((ushort)MsgId.SStartGame, PacketHandler.S_StartGameHandler);		
		_onRecv.Add((ushort)MsgId.SLoadPlayer, MakePacket<S_LoadPlayer>);
		_handler.Add((ushort)MsgId.SLoadPlayer, PacketHandler.S_LoadPlayerHandler);		
		_onRecv.Add((ushort)MsgId.SInvalidName, MakePacket<S_InvalidName>);
		_handler.Add((ushort)MsgId.SInvalidName, PacketHandler.S_InvalidNameHandler);		
		_onRecv.Add((ushort)MsgId.SDuplicateName, MakePacket<S_DuplicateName>);
		_handler.Add((ushort)MsgId.SDuplicateName, PacketHandler.S_DuplicateNameHandler);		
		_onRecv.Add((ushort)MsgId.SDead, MakePacket<S_Dead>);
		_handler.Add((ushort)MsgId.SDead, PacketHandler.S_DeadHandler);		
		_onRecv.Add((ushort)MsgId.SWatchOther, MakePacket<S_WatchOther>);
		_handler.Add((ushort)MsgId.SWatchOther, PacketHandler.S_WatchOtherHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		// 패킷 정보 추출
		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

    public Action<PacketSession,IMessage,ushort> CustomHandler { get; set; }

	// Packet을 만듦
	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		// Unity 클라이언트의 경우 커스텀 핸들러로 넘김
		if(CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		// 서버의 경우 기존 핸들러로 처리 
		else
		{
            Action<PacketSession, IMessage> action = null;
            if (_handler.TryGetValue(id, out action))
                action.Invoke(session, pkt);
        }
	}

	// 특정 Packet 대상 Handler 호출
	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}