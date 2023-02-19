using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Grid CurrentGrid { get; private set; }

    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int MinY { get; set; }
    public int MaxY { get; set; }

    bool[,] _collision; // collision 배열

    // 갈 수 있는 곳인지 체크
    public bool CanGo(Vector3Int cellPos)
    {
        if (cellPos.x < MinX || cellPos.x > MaxX)
            return false;
        if (cellPos.y < MinY || cellPos.y > MaxY)
            return false;

        int x = cellPos.x - MinX;
        int y = MaxY - cellPos.y;
        return !_collision[y, x];
    }

    // 동적으로 Map 로드
    public void LoadMap(int mapId)
    {
        DestroyMap();

        // Map Prefab load
        string mapName = "Map_" + mapId.ToString("000"); // 3자리 숫자로 자동 변환
        GameObject go = Managers.Resource.Instantiate($"Map/{mapName}");
        go.name = "Map";

        GameObject collision = Util.FindChild(go, "Tilemap_wall1", true);
        // collision tile 비활성화
        if (collision != null)
            collision.SetActive(true);

        CurrentGrid = go.GetComponent<Grid>();

        // Collision 관련 파일
        TextAsset txt = Managers.Resource.Load<TextAsset>($"Map/{mapName}"); // .txt 필요없음
        StringReader reader = new StringReader(txt.text);

        // 한 줄씩 Parsing
        MinX = int.Parse(reader.ReadLine());
        MaxX = int.Parse(reader.ReadLine());
        MinY = int.Parse(reader.ReadLine());
        MaxY = int.Parse(reader.ReadLine());

        int xCount = MaxX - MinX + 1;
        int yCount = MaxY - MinY + 1;
        _collision = new bool[yCount, xCount];

        for(int y = 0; y < yCount; y++)
        {
            string line = reader.ReadLine();
            for(int x = 0; x < xCount; x++)
            {
                _collision[y, x] = (line[x] == '1' ? true : false);
            }
        }


        // Tilemap_up 파일
        TextAsset txt_up = Managers.Resource.Load<TextAsset>($"Map/{mapName}_up"); // .txt 필요없음
        StringReader reader_up = new StringReader(txt_up.text);

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

    public void DestroyMap()
    {
        GameObject map = GameObject.Find("Map");
        if(map != null)
        {
            GameObject.Destroy(map);
            CurrentGrid = null;
        }
    }
}
