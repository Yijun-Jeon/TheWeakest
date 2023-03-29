using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Google.Protobuf.Protocol;

public class PlayerList : MonoBehaviour
{
    [SerializeField]
    TMP_Text nickName;
    [SerializeField]
    TMP_Text power;
    [SerializeField]
    SpriteRenderer SPRD;
    int _playerId;

    public void SetInfo(int playerId)
    {
        _playerId = playerId;
        GameObject player = Managers.Object.FindById(playerId);
        nickName.text = player.name;
        power.text = player.GetComponent<PlayerController>().Power.ToString();
    }

    public void setColor(Color color)
    {
        SPRD.color = color;
    }

    public void changeCamera()
    {
        C_WatchOther watchPacket = new C_WatchOther();
        watchPacket.TargetId = _playerId;
        Managers.Network.Send(watchPacket);
        Debug.Log("Send");
    }
}