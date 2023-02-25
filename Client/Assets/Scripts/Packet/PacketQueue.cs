using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;

public class PacketMessage
{
    public ushort Id { get; set; }
    public IMessage Message { get; set; }
}

public class PacketQueue
{
    public static PacketQueue Instance { get; } = new PacketQueue();
    
    Queue<PacketMessage> _pacektQueue = new Queue<PacketMessage>();
    object _lock = new object();

    public void Push(ushort id, IMessage packet)
    {
        lock (_lock)
        {
            _pacektQueue.Enqueue(new PacketMessage() { Id = id, Message = packet});
        }
    }

    public PacketMessage Pop()
    {
        lock (_lock)
        {
            if (_pacektQueue.Count == 0)
                return null;

            return _pacektQueue.Dequeue();
        }
    }

    public List<PacketMessage> PopAll()
    {
        List<PacketMessage> list = new List<PacketMessage>();

        lock (_lock)
        {
            while(_pacektQueue.Count > 0 )
                list.Add(_pacektQueue.Dequeue());
        }

        return list;
    }
}
