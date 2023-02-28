using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

        object _lock = new object();
        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public GameRoom Add(int mapId)
        {
            GameRoom room = new GameRoom();
            room.Init(mapId);
            lock (_lock)
            {
                room.RoomId = _roomId;
                _rooms.Add(room.RoomId, room);
                _roomId++;
            }
            return room;
        }

        public bool Remove(int roomId)
        {
            lock (_lock)
            {
                return _rooms.Remove(roomId);
            }
        }

        public GameRoom Find(int roomId)
        {
            lock (_lock)
            {
                GameRoom room = null;
                if (_rooms.TryGetValue(roomId, out room))
                    return room;
                return null;
            }
        }
    }
}
