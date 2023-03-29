using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Google.Protobuf.Protocol;

public class PlayerResultAdapter : MonoBehaviour
{
    public GameObject playerResultInfo;
    public GameObject winnerPlayerResultInfo;

    public struct PlayerResultInfoStruct
    {
        public string name;
        public int power;
        public bool dead;

        public PlayerResultInfoStruct(string _nickName, int _power, bool _dead)
        {
            name = _nickName;
            power = _power;
            dead = _dead;
        }
    }

    public void UpdateItems()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        List<PlayerResultInfoStruct> playerResultList = new List<PlayerResultInfoStruct>();

        foreach (int playerId in Managers.Object.FindAllPlayers())
        {
            PlayerController player = Managers.Object.FindById(playerId).GetComponent<PlayerController>();
            int power = player.Power;
            string name = player.name;
            bool dead = player.State == PlayerState.Dead;
            playerResultList.Add(new PlayerResultInfoStruct(name, power, dead));
            // 파워 순으로 정렬 
            playerResultList = playerResultList.OrderBy(x => x.power).ToList();
        }

        PlayerResultInfoStruct? winner = null;
        foreach (PlayerResultInfoStruct player in playerResultList)
        {
            GameObject curPlayer = Instantiate<GameObject>(this.playerResultInfo, transform);
            curPlayer.GetComponent<PlayerResultInfo>().powerText.text = player.power.ToString();
            curPlayer.GetComponent<PlayerResultInfo>().nickNameText.text = player.name;
            if (player.dead == false)
            {
                if (winner == null)
                {
                    winner = player;
                }
                curPlayer.GetComponent<PlayerResultInfo>().backgroundImage.color = new Color(0f, 1f, 0f, 0.5f);
                curPlayer.GetComponent<PlayerResultInfo>().skullIcon.enabled = false;
            }
        }

        if (winner != null)
        {
            winnerPlayerResultInfo.GetComponent<PlayerResultInfo>().powerText.text = winner?.power.ToString();
            winnerPlayerResultInfo.GetComponent<PlayerResultInfo>().nickNameText.text = winner?.name;
        }
    }
}