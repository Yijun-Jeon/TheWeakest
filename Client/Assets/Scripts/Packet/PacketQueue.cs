using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketQueue
{
    public static PacketQueue Instance { get; } = new PacketQueue();
    
    Queue<IPacket> _pacektQueue = new Queue<IPacket>();
    object _lock = new object();

    public void Push(IPacket packet)
    {
        lock (_lock)
        {
            _pacektQueue.Enqueue(packet);
        }
    }

    public IPacket Pop()
    {
        lock (_lock)
        {
            if (_pacektQueue.Count == 0)
                return null;

            return _pacektQueue.Dequeue();
        }
    }

    public List<IPacket> PopAll()
    {
        lock (_lock)
        {
            List<IPacket> list = new List<IPacket>();

            while(_pacektQueue.Count > 0 )
                list.Add(_pacektQueue.Dequeue());

            return list;
        }
    }
}
