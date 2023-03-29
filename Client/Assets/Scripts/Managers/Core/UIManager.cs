using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager
{
    double _endTime;

    #region NOT_USE
    int _order = 10;

    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    UI_Scene _sceneUI = null;

    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
                root = new GameObject { name = "@UI_Root" };
            return root;
        }
    }

    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }

    public T MakeWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/WorldSpace/{name}");
        if (parent != null)
            go.transform.SetParent(parent);

        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        return Util.GetOrAddComponent<T>(go);
    }

    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/SubItem/{name}");
        if (parent != null)
            go.transform.SetParent(parent);

        return Util.GetOrAddComponent<T>(go);
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
        T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;

        go.transform.SetParent(Root.transform);

        return sceneUI;
    }

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        go.transform.SetParent(Root.transform);

        return popup;
    }

    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupStack.Count == 0)
            return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!");
            return;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Popup popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;
        _order--;
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    public void Clear()
    {
        CloseAllPopupUI();
        _sceneUI = null;
    }
    #endregion
    public void OnConnectSuccess(bool isConnected)
    {
        if (isConnected == false)
            return;

        AlertMessage("서버 접속에 성공했습니다.");
    }

    public void AlertMessage(string message)
    {
        GameObject.FindWithTag("Alert").GetComponent<AlertListAdapter>().AddAlert(message);
    }

    public void UpdatePlayerList()
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("PlayerListPanel").
            transform.Find("PlayerList").GetComponent<PlayerListAdapter>().UpdateList();
    }

    public void UpdateKillFeed(int playerId)
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").
            transform.Find("KillList").GetComponent<KillInfoAdapter>().AddKillInfo(playerId);
    }

    public void UpdateTargetPlayer(int playerId)
    {
        PlayerController targetPlayer = Managers.Object.FindById(playerId).GetComponent<PlayerController>();
        Camera.main.GetComponent<CameraController>().SetTargetPlayer(targetPlayer);
    }

    public void UpdateWeakestText(string name)
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("WeakestText").GetComponent<TMP_Text>().text = "현재 꼴등 : " + name;
    }

    public void UpdateRunText(bool isMe)
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("RunText").gameObject.SetActive(isMe);
    }

    public void UpdateRemainText(int playerCount, int aliveCount)
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("RemainText").GetComponent<TMP_Text>().text = aliveCount + "/" + playerCount;
    }

    public void ActivePlayerListPanel(bool active)
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("PlayerListPanel").gameObject.SetActive(active);
    }

    public void ActiveInGamePanel(bool active)
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").gameObject.SetActive(active);
    }

    public void ActiveStartBtn(bool active)
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("PlayerListPanel").transform.Find("StartBtn").gameObject.SetActive(active);
    }

    public void ActiveCancelBtn(bool active)
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("PlayerListPanel").transform.Find("CancelBtn").gameObject.SetActive(false);
    }

    public void UpdateViewDistance(float speed)
    {
        Camera.main.GetComponent<CameraController>().UpdateViewDistance(speed);
    }

    public void UpdateTime(double time)
    {
        if (_endTime == time)
            return;

        _endTime = time;
        string minuteText = ((int)time / 60 % 60).ToString();
        string secondText = ((int)time % 60).ToString();
        Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("TimerText").GetComponent<TMP_Text>().text = $"{minuteText} : {secondText.PadLeft(2,'0')}";
        if(time < 30f)
        {
            Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("TimerText").GetComponent<TMP_Text>().color = Color.red;
        }
    }
}