using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class AlertListAdapter : MonoBehaviourPunCallbacks
{

    public GameObject contents;
    public GameObject alertText;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void AddAlert(string messasge)
    {
        GameObject curAlert = Instantiate<GameObject>(this.alertText, contents.transform);
        curAlert.GetComponent<AlertText>().SetMessage(messasge);
        Destroy(curAlert, 5);
    }

    // Update is called once per frame
    void Update()
    {

    }

}
