using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillInfoAdapter : MonoBehaviour
{
    [SerializeField]
    public GameObject contents;
    [SerializeField]
    public GameObject killList;

    public void AddKillInfo(int playerId)
    {
        GameObject curKill = Instantiate(this.killList, contents.transform);
        curKill.GetComponent<KillInfo>().SetPlayerInfo(playerId);
        Destroy(curKill, 5f);
    }
}
