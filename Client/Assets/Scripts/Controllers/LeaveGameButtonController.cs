using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;
using UnityEngine.SceneManagement;

public class LeaveGameButtonController : MonoBehaviour
{
    public void Leave()
    {
        C_LeaveGame leaveGamePacket = new C_LeaveGame();
        leaveGamePacket.PlayerId = Managers.Object.MyPlayer.Id;
        Managers.Network.Send(leaveGamePacket);
    }
}