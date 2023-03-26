using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Google.Protobuf.Protocol;

public class PlayerListAdapter : MonoBehaviour
{

    public GameObject contents;
    public GameObject playerInfo;

    public void UpdateList()
    {
        foreach (Transform child in contents.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (int playerId in Managers.Object.FindAllPlayers())
        {
            GameObject curPlayer = Instantiate<GameObject>(this.playerInfo, contents.transform);
            curPlayer.GetComponent<PlayerList>().SetInfo(playerId);

            // TODO : 사망시 색깔 처리 
            //object curProp;
            //if (player.CustomProperties.TryGetValue("dead", out curProp) && (bool)curProp)
            //    curPlayer.GetComponent<PlayerInfo>().setColor(Color.grey);
            //else
            //    curPlayer.GetComponent<PlayerInfo>().setColor(Color.red);
        }
    }
}