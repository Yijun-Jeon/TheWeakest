using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Google.Protobuf.Protocol;
using UnityEngine.SceneManagement;

public class EnterButtonController : MonoBehaviour
{
    TMP_InputField nameInput;

    public void Start()
    {
        nameInput = Camera.main.transform.Find("CameraCanvas").transform.Find("DisconnectPanel").transform.Find("NameInput").GetComponent<TMP_InputField>();
    }

    public void Enter()
    {
        string name = nameInput.text;

        C_EnterGame enterGamePacket = new C_EnterGame();
        enterGamePacket.Name = name;
        Managers.Network.Send(enterGamePacket);
    }
}
