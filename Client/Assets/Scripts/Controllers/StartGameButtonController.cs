using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class StartGameButtonController : MonoBehaviour
{
    public void Enter()
    {
        if(Managers.Object.FindAllPlayers().Count == 1)
        {
            Managers.Network.AlertMessage("혼자서는 게임을 시작할 수 없습니다.");
            return;
        }
            
        C_StartGame startGamePacket = new C_StartGame();
        Managers.Network.Send(startGamePacket);
    }
}
