using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server
{
    // Unity의 Vector3Int 대신 
    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y) { this.x = x; this.y = y; }

        public static Vector2Int up { get { return new Vector2Int(0, 1); } }
        public static Vector2Int down { get { return new Vector2Int(0, -1); } }
        public static Vector2Int left { get { return new Vector2Int(-1, 0); } }
        public static Vector2Int right { get { return new Vector2Int(1, 0); } }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x + b.x, a.y + b.y);
        }
    }

    public class Map
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }

        public int SizeX { get { return MaxX - MinX + 1; } }
        public int SizeY { get { return MaxY - MinY + 1; } }

        // 벽 여부
        bool[,] _collision;
        // Player 여부
        Player[,] _players;

        public bool CanGo(Vector2Int cellPos, bool checkObjects = true)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return false;
            if (cellPos.y < MinY || cellPos.y > MaxY)
                return false;

            int x = cellPos.x - MinX;
            int y = MaxY - cellPos.y;
            return !_collision[y, x] && (!checkObjects || _players[y, x] == null);
        }

        public bool ApplyMove(Player player, Vector2Int dest)
        {
            PositionInfo posInfo = player.Info.PosInfo;
            if (posInfo.PosX < MinX || posInfo.PosX > MaxX)
                return false;
            if (posInfo.PosY < MinX || posInfo.PosY > MaxX)
                return false;
            if (CanGo(dest, true) == false)
                return false;
            // 플레이어가 원래 있던 위치 비워줌
            {
                int x = posInfo.PosX - MinX;
                int y = MaxY - posInfo.PosY;
                if (_players[y, x] == player)
                    _players[y, x] = null;

            }
            // 목표 위치로 이전
            {
                int x = dest.x - MinX;
                int y = MaxY - dest.y;
                _players[y, x] = player;
            }

            return true;
        }

        public void LoadMap(int mapId, string pathPrefix)
        {
            // Map Prefab load
            string mapName = "Map_" + mapId.ToString("000"); // 3자리 숫자로 자동 변환

            // Collision 관련 파일
            string txt = File.ReadAllText($"{pathPrefix}/{mapName}.txt"); 
            StringReader reader = new StringReader(txt);

            // 한 줄씩 Parsing
            MinX = int.Parse(reader.ReadLine());
            MaxX = int.Parse(reader.ReadLine());
            MinY = int.Parse(reader.ReadLine());
            MaxY = int.Parse(reader.ReadLine());

            int xCount = MaxX - MinX + 1;
            int yCount = MaxY - MinY + 1;
            _collision = new bool[yCount, xCount];

            for (int y = 0; y < yCount; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < xCount; x++)
                {
                    _collision[y, x] = (line[x] == '1' ? true : false);
                }
            }


            // Tilemap_up 파일
            string txt_up = File.ReadAllText($"{pathPrefix}/{mapName}_up.txt");
            StringReader reader_up = new StringReader(txt_up);

            // 한 줄씩 Parsing
            reader_up.ReadLine();
            reader_up.ReadLine();
            reader_up.ReadLine();
            reader_up.ReadLine();

            for (int y = 0; y < yCount; y++)
            {
                string line = reader_up.ReadLine();
                for (int x = 0; x < xCount; x++)
                {
                    if (_collision[y, x] == true)
                        continue;
                    _collision[y, x] = (line[x] == '1' ? true : false);
                }
            }
        }
    }
}