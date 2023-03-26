using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerList : MonoBehaviour
{
    [SerializeField]
    TMP_Text nickName;
    [SerializeField]
    TMP_Text power;

    public void SetInfo(int playerId)
    {
        GameObject player = Managers.Object.FindById(playerId);
        nickName.text = player.name;
        power.text = player.GetComponent<PlayerController>().Power.ToString();
    }

    //public void setColor(Color color)
    //{
    //    SPRD.color = color;
    //}

    public void changeCamera()
    {
        // TODO
    }
}