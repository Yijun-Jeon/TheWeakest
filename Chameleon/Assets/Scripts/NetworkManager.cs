using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    Dictionary<int, GameObject> prefabDict;
    public TMP_InputField NickNameInput;
    public GameObject DisconnectPanel;
    public GameObject ReadyPanel;
    public GameObject InGamePanel;
    public GameObject EndPanel;
    public GameObject Black;
    public Camera MainCamera;
    // public GameObject masterText;


    private bool start = false;
    private bool end = false;
    private double startTime;
    private double endTime;



    void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    public void Connect()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print("nonono");
        Camera.main.transform.Find("CameraCanvas").transform.Find("DisconnectPanel").transform.Find("AlertList").GetComponent<AlertListAdapter>().
        AddAlert("게임이 이미 진행 중입니다.");

    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }


    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        start = false;
        end = false;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 20 }, null);
    }

    public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(false);
        Black.SetActive(true);
        Camera.main.transform.Find("CameraCanvas").transform.Find("ReadyPanel").gameObject.SetActive(true);
        Camera.main.transform.Find("CameraCanvas").transform.Find("ReadyPanel").transform.Find("StartBtn").gameObject.SetActive(true);
        Camera.main.transform.Find("CameraCanvas").transform.Find("ReadyPanel").transform.Find("CancelBtn").gameObject.SetActive(true);
        if (!PhotonNetwork.IsMasterClient)
            Camera.main.transform.Find("CameraCanvas").transform.Find("ReadyPanel").transform.Find("StartBtn").GetComponent<Button>().interactable = false;
        else
            Camera.main.transform.Find("CameraCanvas").transform.Find("ReadyPanel").transform.Find("StartBtn").GetComponent<Button>().interactable = true;
        MainCamera.orthographicSize = 7;
        start = false;
        end = false;
        Spawn();
    }

    public override void OnCreatedRoom()
    {
        start = false;
        end = false;
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // if (PhotonNetwork.IsMasterClient)
        // {
        //     PhotonNetwork.Destroy(PhotonNetwork.GetPhotonView((int)otherPlayer.CustomProperties["PVID"]));

        // }
        if (!start && PhotonNetwork.IsMasterClient)
        {
            // masterText.SetActive(true);
            // Color textColor = masterText.GetComponent<TMP_Text>().color;
            // textColor.a = 1;
            // masterText.GetComponent<TMP_Text>().color = textColor;
            ReadyPanel.transform.Find("StartBtn").GetComponent<Button>().interactable = true;
        }
        CheckVictoryCondition();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    public Dictionary<int, Player> GetPlayerInfo()
    {
        return PhotonNetwork.CurrentRoom.Players;
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
        // {
        //     PhotonNetwork.LeaveRoom();
        //     PhotonNetwork.Disconnect();
        //     return;
        // }
        if (start)
        {
            double time = PhotonNetwork.Time;
            if (time >= endTime)
            {
                if (end == false)
                {
                    Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("TimerText").GetComponent<TMP_Text>().text = $"게임 종료!";
                    Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").gameObject.SetActive(false);
                    if (PhotonNetwork.IsMasterClient)
                    {
                        Hashtable room_cp = PhotonNetwork.CurrentRoom.CustomProperties;
                        room_cp["end"] = true;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(room_cp);
                    }
                }
            }
            else
            {
                double timeLeft = endTime - time;
                string minute_text = ((int)timeLeft / 60 % 60).ToString();
                string second_text = ((int)timeLeft % 60).ToString();
                InGamePanel.transform.Find("TimerText").GetComponent<TMP_Text>().text = $"{minute_text} : {second_text.PadLeft(2, '0')}";
                if (timeLeft < 30f)
                {
                    InGamePanel.transform.Find("TimerText").GetComponent<TMP_Text>().color = Color.red;
                }
                else
                {
                    InGamePanel.transform.Find("TimerText").GetComponent<TMP_Text>().color = Color.white;
                }
            }
        }

    }

    public void Spawn()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(-4.2f + Random.Range(-1, 1), 0.8f + Random.Range(-1, 1), 0), Quaternion.identity);
        DisconnectPanel.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            // Camera.main.transform.Find("CameraCanvas").transform.Find("MasterText").gameObject.SetActive(true);
            // GameObject.FindGameObjectWithTag("MasterText").SetActive(true);
            // masterText.SetActive(true);
            // Color textColor = masterText.GetComponent<TMP_Text>().color;
            // textColor.a = 1;
            // masterText.GetComponent<TMP_Text>().color = textColor;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Camera.main.transform.Find("CameraCanvas").transform.Find("DisconnectPanel").gameObject.SetActive(true);
        // Camera.main.transform.Find("CameraCanvas").transform.Find("MasterText").gameObject.SetActive(false);
        // masterText.SetActive(false);
        // Color textColor = masterText.GetComponent<TMP_Text>().color;
        // textColor.a = 0;
        // masterText.GetComponent<TMP_Text>().color = textColor;
        // GameObject.FindGameObjectWithTag("MasterText").SetActive(false);
        Camera.main.transform.Find("CameraCanvas").transform.Find("ReadyPanel").gameObject.SetActive(false);
        Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("AlramText").gameObject.SetActive(false);
        Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").gameObject.SetActive(false);
        Camera.main.transform.Find("CameraCanvas").transform.Find("EndPanel").gameObject.SetActive(false);
    }

    public void startGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Camera.main.transform.Find("CameraCanvas").transform.Find("ReadyPanel").transform.Find("AlertList").GetComponent<AlertListAdapter>().
            AddAlert("방장만 게임을 시작할 수 있습니다.");
            return;
        }

        int numOfPlayers = PhotonNetwork.PlayerList.Length;

        if (numOfPlayers == 1)
        {
            Camera.main.transform.Find("CameraCanvas").transform.Find("ReadyPanel").transform.Find("AlertList").GetComponent<AlertListAdapter>().
            AddAlert("혼자서는 게임을 진행할 수 없습니다.");
            return;

        }

        List<Vector3> SpawnSpaces = new List<Vector3>();
        SpawnSpaces.Add(new Vector3(-27, 6, 0)); SpawnSpaces.Add(new Vector3(-4.2f, 0.8f, 0)); SpawnSpaces.Add(new Vector3(11f, -4f, 0));
        SpawnSpaces.Add(new Vector3(25, 0, 0)); SpawnSpaces.Add(new Vector3(40f, 5.5f, 0)); SpawnSpaces.Add(new Vector3(52f, 5.19f, 0));
        SpawnSpaces.Add(new Vector3(77.61f, -4f, 0)); SpawnSpaces.Add(new Vector3(105.54f, 16.39f, 0)); SpawnSpaces.Add(new Vector3(36.21f, 20.97f, 0));
        SpawnSpaces.Add(new Vector3(35f, 37.33f, 0)); SpawnSpaces.Add(new Vector3(19.58f, 17f, 0)); SpawnSpaces.Add(new Vector3(5f, 22.05f, 0));
        SpawnSpaces.Add(new Vector3(3.76f, 26.63f, 0)); SpawnSpaces.Add(new Vector3(9.02f, 17.38f, 0)); SpawnSpaces.Add(new Vector3(-9.26f, 18.11f, 0));
        SpawnSpaces.Add(new Vector3(2.07f, 30.76f, 0)); SpawnSpaces.Add(new Vector3(56.3f, 14.8f, 0)); SpawnSpaces.Add(new Vector3(2.89f, 14.21f, 0));
        SpawnSpaces.Add(new Vector3(74.8f, 6f, 0)); SpawnSpaces.Add(new Vector3(70.12f, -4f, 0));

        for (int i = 0; i < SpawnSpaces.Count; i++)
        {
            Vector3 fruitCurrentIndex = SpawnSpaces[i];
            int randomIndex = Random.Range(i, SpawnSpaces.Count);
            SpawnSpaces[i] = SpawnSpaces[randomIndex];
            SpawnSpaces[randomIndex] = fruitCurrentIndex;
        }

        // else
        PhotonNetwork.CurrentRoom.IsOpen = false;


        int[] intArr = new int[numOfPlayers];

        for (int i = 0; i < numOfPlayers; i++)
            intArr[i] = i + 1;

        int random1, random2;
        int temp;

        for (int i = 0; i < intArr.Length; i++)
        {
            random1 = Random.Range(0, intArr.Length);
            random2 = Random.Range(0, intArr.Length);

            temp = intArr[random1];
            intArr[random1] = intArr[random2];
            intArr[random2] = temp;
        }

        int index = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // Hashtable player_cp = new Hashtable();
            Hashtable player_cp = player.CustomProperties;
            player_cp["dead"] = false;
            player_cp["power"] = intArr[index];
            // player_cp["power"] = player.IsLocal ? -1 : intArr[index];
            player_cp["space"] = SpawnSpaces[intArr[index]];
            player.SetCustomProperties(player_cp);
            index++;
        }
        Hashtable room_cp = new Hashtable();
        room_cp.Add("start", true);
        double startTime = PhotonNetwork.Time;
        room_cp.Add("startTime", startTime);
        // double playTime = 100f + 10 * numOfPlayers;
        double playTime = 90f;
        room_cp.Add("endTime", startTime + playTime);
        room_cp.Add("end", false);
        PhotonNetwork.CurrentRoom.SetCustomProperties(room_cp);
        // masterText.SetActive(false);
    }


    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        bool prevStart = start;
        object propsStart;
        if (propertiesThatChanged.TryGetValue("start", out propsStart))
        {
            start = (bool)propsStart;
        }
        object propsStartTime;
        if (propertiesThatChanged.TryGetValue("startTime", out propsStartTime))
        {
            startTime = (double)propsStartTime;
            endTime = (double)PhotonNetwork.CurrentRoom.CustomProperties["endTime"];
            if (start == true && start != prevStart)
            {
                Camera.main.transform.Find("CameraCanvas").transform.Find("ReadyPanel").gameObject.SetActive(false);
                Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").gameObject.SetActive(true);
                Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("MinText").gameObject.SetActive(true);
                Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("KillText").gameObject.SetActive(true);
            }
        }
        object propsEnd;
        if (propertiesThatChanged.TryGetValue("end", out propsEnd))
        {
            bool prevEnd = end;
            end = (bool)propsEnd;
            if (end == true && end != prevEnd)
            {
                Camera.main.transform.Find("CameraCanvas").transform.Find("InGamePanel").gameObject.SetActive(false);
                Camera.main.transform.Find("CameraCanvas").transform.Find("EndPanel").gameObject.SetActive(true);
                Camera.main.transform.Find("CameraCanvas").transform.Find("EndPanel").transform.Find("PlayerResultLayout").GetComponent<PlayerResultListAdapter>().UpdateItems();
                Invoke(nameof(AutoDisconnect), 10f);
            }
        }
    }
    public void AutoDisconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        CheckVictoryCondition();
    }
    public void CheckVictoryCondition()
    {
        int numAlive = 0;
        if (start && PhotonNetwork.IsMasterClient && end == false)
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                bool dead = (bool)player.CustomProperties["dead"];
                if (!dead) numAlive++;
            }
            if (numAlive <= 1)
            {
                Hashtable room_cp = PhotonNetwork.CurrentRoom.CustomProperties;
                room_cp["end"] = true;
                PhotonNetwork.CurrentRoom.SetCustomProperties(room_cp);
            }
        }
    }

}
