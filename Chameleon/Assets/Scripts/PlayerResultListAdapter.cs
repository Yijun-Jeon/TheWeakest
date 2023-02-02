using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class PlayerResultListAdapter : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject playerResultInfo;
    [SerializeField] private GameObject winnerPlayerResultInfo;

    public struct PlayerResultInfoStruct
    {
        public string nickName;
        public int power;
        public bool dead;

        public PlayerResultInfoStruct(string _nickName, int _power, bool _dead)
        {
            nickName = _nickName;
            power = _power;
            dead = _dead;
        }
    }
    void Start()
    {
        // UpdateItems();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void UpdateItems()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        List<PlayerResultInfoStruct> playerResultList = new List<PlayerResultInfoStruct>();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int power = (int)player.CustomProperties["power"];
            string nickName = player.NickName;
            bool dead = (bool)player.CustomProperties["dead"];
            playerResultList.Add(new PlayerResultInfoStruct(nickName, power, dead));
            playerResultList = playerResultList.OrderBy(x => x.power).ToList();
        }
        PlayerResultInfoStruct? winner = null;
        foreach (PlayerResultInfoStruct item in playerResultList)
        {
            GameObject curPlayer = Instantiate<GameObject>(this.playerResultInfo, transform);
            curPlayer.GetComponent<PlayerResultInfo>().powerText.text = item.power.ToString();
            curPlayer.GetComponent<PlayerResultInfo>().nickNameText.text = item.nickName;
            if(!item.dead)
            {
                if(winner == null)
                {
                    winner = item;
                }
                curPlayer.GetComponent<PlayerResultInfo>().backgroundImage.color = new Color(0f, 1f, 0f, 0.5f);
                curPlayer.GetComponent<PlayerResultInfo>().skullIcon.enabled = false;
            }
        }
        if(winner != null)
        {
            winnerPlayerResultInfo.GetComponent<PlayerResultInfo>().powerText.text = winner?.power.ToString();
            winnerPlayerResultInfo.GetComponent<PlayerResultInfo>().nickNameText.text = winner?.nickName;
        }
    }
}
