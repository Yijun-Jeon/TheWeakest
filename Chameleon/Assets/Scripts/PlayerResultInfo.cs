using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerResultInfo : MonoBehaviour
{
    public TMP_Text powerText;
    public TMP_Text nickNameText;
    public TMP_Text survivalTimeText;
    public Image backgroundImage;
    // Start is called before the first frame update
    public Image skullIcon;
    void Start()
    {
        // powerText = transform.Find("PowerText").GetComponent<TMP_Text>();
        // nickNameText = transform.Find("NicknameText").GetComponent<TMP_Text>();
        // survivalTimeText = transform.Find("SurvivalTimeText").GetComponent<TMP_Text>();
        // backgroundImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
