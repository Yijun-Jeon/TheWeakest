using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : Player
{
    NetworkManager _network;
    
    void Start()
    {
        _network = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        StartCoroutine("CoSendPacket");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator CoSendPacket()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);

            C_Move movePacket = new C_Move();
            movePacket.posX = Random.Range(-9, 9);
            movePacket.posY = Random.Range(-4, 4);
            movePacket.posZ = 0;

            _network.Send(movePacket.Write());
        }
    }
}
