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
            GameObject go = Instantiate<GameObject>(this.playerInfo, contents.transform);
            go.GetComponent<PlayerList>().SetInfo(playerId);

            PlayerController player = Managers.Object.FindById(playerId).GetComponent<PlayerController>();

            // 사망 시 회색 처리 
            if (player.State == PlayerState.Dead)
                go.GetComponent<PlayerList>().setColor(Color.grey);
        }
    }
}