using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

// 개발하는 단계에서만 생성되게 설정 
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapEditor : MonoBehaviour
{
#if UNITY_EDITOR

    // Unity 상의 메뉴에 표시
    // 기존 메뉴 산하도 가능
    // 단축기 % (Ctrl), # (Shift), &(Alt)
    [MenuItem("Tools/GenerateMap %#g")]
    private static void GenerateMap()
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");

        foreach(GameObject go in gameObjects)
        {
            Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_wall1", true);

            // 파일 포맷 - binnary or text
            using (var writter = File.CreateText($"Assets/Resources/Map/{go.name}.txt"))
            {
                writter.WriteLine(tm.cellBounds.xMin);
                writter.WriteLine(tm.cellBounds.xMax);
                writter.WriteLine(tm.cellBounds.yMin);
                writter.WriteLine(tm.cellBounds.yMax);

                for(int y = tm.cellBounds.yMax; y >= tm.cellBounds.yMin; y--)
                {
                    for(int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                        {
                            writter.Write("1");
                        }
                        else
                            writter.Write("0");
                    }
                    writter.WriteLine();
                }
            }
        }
    }

#endif
}
