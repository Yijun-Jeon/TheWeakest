using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertListAdapter : MonoBehaviour
{
    [SerializeField]
    public GameObject contents;
    [SerializeField]
    public GameObject alertText;

    public void AddAlert(string message)
    {
        GameObject curAlert = Instantiate(this.alertText, contents.transform);
        curAlert.GetComponent<AlertText>().SetMessage(message);
        Destroy(curAlert, 5);
    }
}
