using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlertText : MonoBehaviour
{
    public TMP_Text message;
    // Start is called before the first frame update
    void Start()
    {
        message = GetComponent<TMP_Text>();
    }

    public void SetMessage(string message)
    {
        this.message.text = message;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
