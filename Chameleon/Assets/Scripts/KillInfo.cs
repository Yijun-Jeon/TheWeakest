using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class KillInfo : MonoBehaviour
{
    public TMP_Text nickName;
    public TMP_Text power;
    Player player;
    
    void Start()
    {
        nickName = GetComponent<TMP_Text>();
        power = GetComponent<TMP_Text>();
    }

    public void SetNickName(Player player)
    {
        this.nickName.text = player.NickName;
        this.player = player;
        this.power.text = player.CustomProperties["power"].ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
