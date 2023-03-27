using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillInfo : MonoBehaviour
{
    public TMP_Text nickName;
    public TMP_Text power;

    void Start()
    {
        nickName = GetComponent<TMP_Text>();
        power = GetComponent<TMP_Text>();
    }

    public void SetPlayerInfo(int playerId)
    {
        GameObject player = Managers.Object.FindById(playerId);
        nickName.text = player.name;
        power.text = player.GetComponent<PlayerController>().Power.ToString();
    }

    
    void Update()
    {
        
    }
}
