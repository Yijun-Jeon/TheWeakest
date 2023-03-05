using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using TMPro;

public class ObjectManager 
{
    public MyPlayerController MyPlayer { get; set; }
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    public void Add(PlayerInfo info, bool myPlayer = false)
    {
        // 내 플레이어
        if (myPlayer)
        {
            GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
            go.name = info.Name;
            go.transform.Find("Canvas").transform.Find("NickNameText").GetComponent<TMP_Text>().SetText(info.Name);
            _objects.Add(info.PlayerId, go);

            MyPlayer = go.GetComponent<MyPlayerController>();
            MyPlayer.Id = info.PlayerId;
            MyPlayer.PosInfo = info.PosInfo;

            MyPlayer.SyncsPos();
        }
        // 다른 플레이어 
        else
        {
            GameObject go = Managers.Resource.Instantiate("Creature/Player");
            go.name = info.Name;
            go.transform.Find("Canvas").transform.Find("NickNameText").GetComponent<TMP_Text>().SetText(info.Name);
            _objects.Add(info.PlayerId, go);

            PlayerController pc = go.GetComponent<PlayerController>();
            pc.Id = info.PlayerId;
            pc.PosInfo = info.PosInfo;

            pc.SyncsPos();
        }
    }

    public void Add(int id, GameObject go)
    {
        _objects.Add(id, go);
    }

    public void Remove(int id)
    {
        GameObject go = FindById(id);
        if (go == null)
            return;

        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public void RemoveMyPlayer()
    {
        if (MyPlayer == null)
            return;

        Remove(MyPlayer.Id);
        MyPlayer = null;
    }

    public GameObject Find(Func<GameObject,bool> condition)
    {
        foreach(GameObject obj in _objects.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }
        return null;
    }

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
            Managers.Resource.Destroy(obj);

        _objects.Clear();
    }
}
