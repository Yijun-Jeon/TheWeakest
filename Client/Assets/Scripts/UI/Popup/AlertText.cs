using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AlertText : MonoBehaviour
{
    public TMP_Text message;

    void Start()
    {
        message = GetComponent<TMP_Text>();        
    }

    public void SetMessage(string message)
    {
        this.message.text = message;
    }

    
    void Update()
    {
        
    }
}
