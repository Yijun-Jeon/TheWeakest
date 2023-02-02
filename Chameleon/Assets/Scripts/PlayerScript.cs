using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D RB;
    public Animator AN;
    public SpriteRenderer SR;
    public PhotonView PV;
    public TMP_Text NickNameText;
    [SerializeField] private AttackRange attackRange;
    [SerializeField] private FieldOfView fieldOfView;
    public TMP_Text PowerText;
    [SerializeField] private float moveSpeed;
    Vector3 curPos;

    private float curTime;
    public float coolTime = 1.7f;
    private float curTime_fake;
    public float coolTime_fake = 6f;
    public Transform pos;
    public int power;
    [SerializeField] public TMP_Text AlarmText;
    public bool isControl;
    public bool canTeleport;
    public Transform teleportTarget;
    private bool isMin;
    private bool isSpawn;
    private bool isDead = false;
    private bool isStart = false;
    public string minName = " ";
    public int kill = 0;
    public GameObject info;


    void Awake()
    {
        Hashtable player_cp = PV.Owner.CustomProperties;
        player_cp["power"] = 0;
        player_cp["PVID"] = PV.ViewID;
        player_cp["dead"] = false;
        player_cp["space"] = Vector3.zero;
        PV.Owner.SetCustomProperties(player_cp);

        // GameObject.Find("PlayerList").GetComponent<PlayerListAdapter>().setTF(PV.Owner.ActorNumber, transform);

        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;
        attackRange = transform.Find("AttackRange").gameObject.GetComponent<AttackRange>();
        if (PV.IsMine)
            fieldOfView = GameObject.Find("FieldOfView").GetComponent<FieldOfView>();
        isControl = true;
        isMin = false;
        isSpawn = true;
        canTeleport = false;
        teleportTarget = null;
        System.Random rand = new System.Random();

        if (PV.IsMine)
        {
            PowerText.color = new Color(0, 0, 0, 0);
            Camera.main.GetComponent<CameraController>().targetRB = RB;
            Camera.main.GetComponent<CameraController>().targetTF = transform;
        }
        else
        {
            transform.Find("AttackRange").gameObject.SetActive(false);
        }
        UpdatePlayerStatus();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (targetPlayer.IsLocal && (bool)targetPlayer.CustomProperties["dead"] && !isDead)
            isDead = true;

        if (targetPlayer.IsLocal && PV.IsMine)
        {
            PowerText.text = targetPlayer.CustomProperties["power"].ToString();
            power = Convert.ToInt32(targetPlayer.CustomProperties["power"]);
            if (isSpawn && !isStart)
            {
                Vector3 curSpace = (Vector3)targetPlayer.CustomProperties["space"];
                if (!curSpace.Equals(Vector3.zero))
                    this.transform.position = curSpace;
            }
        }
        UpdatePlayerStatus();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        bool prevIsStart = isStart;
        isStart = Convert.ToBoolean(PhotonNetwork.CurrentRoom.CustomProperties["start"]);
        if (isStart)
        {
            if (!isDead && PV.IsMine)
            {
                Camera.main.GetComponent<CameraController>().targetRB = RB;
                Camera.main.GetComponent<CameraController>().targetTF = transform;
            }

            int min = 20;
            int remainCnt = 0;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                Hashtable table = player.CustomProperties;
                if (!(Convert.ToBoolean(table["dead"])))
                {
                    remainCnt++;
                    if (Convert.ToInt32(table["power"]) < min)
                    {
                        min = Convert.ToInt32(table["power"]);
                        minName = player.NickName;
                    }
                }
            }
            if (min == power)
                isMin = true;
            GameObject.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("MinText").GetComponent<TMP_Text>().text = "현재 꼴등 : " + minName;
            GameObject.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("RemainText").GetComponent<TMP_Text>().text = remainCnt + "/" + PhotonNetwork.PlayerList.Length;
        }
        if (prevIsStart != isStart)
        {
            UpdatePlayerStatus();
        }
    }

    void Update()
    {
        if (PV.IsMine)
        {
            // fieldOfView.SetOrigin(transform.position);
            attackRange.SetOrigin(transform.position);
            GameObject.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("KillText").GetComponent<TMP_Text>().text = "Kill : " + kill.ToString();

            // 샤킹중이면 아무것도 못함
            if (curTime_fake > 0)
            {
                RB.velocity = Vector2.zero;
                curTime_fake -= Time.deltaTime;
            }
            else
            {
                if (!AN.GetBool("dead"))
                {
                    if (isControl)
                    {
                        if(canTeleport && Input.GetKeyDown(KeyCode.LeftAlt))
                        {
                            StartCoroutine(nameof(TeleportRoutine));
                        }
                        float xAxis = Input.GetAxisRaw("Horizontal");
                        float yAxis = Input.GetAxisRaw("Vertical");
                        RB.velocity = new Vector2(xAxis, yAxis).normalized * moveSpeed;

                        if (xAxis != 0 || yAxis != 0)
                        {
                            Camera.main.GetComponent<CameraController>().targetRB = RB;
                            Camera.main.GetComponent<CameraController>().targetTF = transform;
                            AN.SetBool("walk", true);
                            SR.flipX = (xAxis == -1);
                            // PV.RPC(nameof(FlipXRPC), RpcTarget.AllBuffered, xAxis);
                        }
                        else AN.SetBool("walk", false);
                    }

                    Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(pos.position, 1.5f);
                    bool activated = false;
                    foreach (Collider2D collider in collider2Ds)
                    {
                        if (collider.tag == "Player" && !collider.GetComponent<PlayerScript>().PV.IsMine)
                        {
                            activated = true;
                            break;
                        }
                    }
                    attackRange.SetColor(activated);
                    if (curTime <= 0)
                    {   //공격
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            foreach (Collider2D collider in collider2Ds)
                            {
                                if (power != 0 && collider.tag == "Player" && !collider.GetComponent<PlayerScript>().PV.IsMine
                                                                    && !collider.GetComponent<PlayerScript>().AN.GetBool("dead"))
                                {
                                    if (Convert.ToInt32(collider.GetComponent<PlayerScript>().PowerText.text) < power)
                                    {
                                        collider.GetComponent<PlayerScript>().MakeDead();
                                        PV.RPC("IncreKillRPC", RpcTarget.All);
                                    }
                                    else if (Convert.ToInt32(collider.GetComponent<PlayerScript>().PowerText.text) > power)
                                    {
                                        MakeDead();
                                        collider.GetComponent<PlayerScript>().PV.RPC("IncreKillRPC", RpcTarget.All);
                                    }
                                }
                            }
                            PV.RPC("AttackRPC", RpcTarget.All);
                            curTime = coolTime;
                        }
                    }
                    else
                    {
                        curTime -= Time.deltaTime;
                    }

                    if (Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        PV.RPC("FakeRPC", RpcTarget.All);
                        attackRange.SetColor(false);
                        curTime_fake = coolTime_fake;
                    }
                }
                else // dead
                {
                    RB.velocity = Vector2.zero;
                    PowerText.color = Color.white;
                    attackRange.SetColor(false);
                    GameObject.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("AlramText").gameObject.SetActive(false);
                    isMin = false;
                }
            }
        }// !PV.IsMine
        else if ((transform.position - curPos).sqrMagnitude >= 100)
        {
            transform.position = curPos;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
        }
    }


    public void MakeDead()
    {
        PV.RPC("DeadRPC", RpcTarget.AllBuffered);
        Hashtable player_cp = PV.Owner.CustomProperties;
        player_cp["dead"] = true;
        PV.Owner.SetCustomProperties(player_cp);

    }

    [PunRPC]
    void FlipXRPC(float xAxis)
    {
        if (xAxis == -1) SR.flipX = true;
        else if (xAxis == 1) SR.flipX = false;
    }
    [PunRPC]
    void IncreKillRPC()
    {
        kill++;
    }
    [PunRPC]
    void AttackRPC()
    {
        AN.SetTrigger("attack");
    }

    [PunRPC]
    void DeadRPC()
    {
        RB.velocity = Vector2.zero;
        AN.SetBool("walk", false);
        AN.SetBool("dead", true);
        GameObject kill = Instantiate<GameObject>(this.info, GameObject.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("KillList").transform.Find("ScrollView").transform.Find("Viewport").transform.Find("Contents").transform);
        kill.GetComponent<KillInfo>().SetNickName(PV.Owner);
        Destroy(kill, 5f);
        if (PV.IsMine)
        {
            Transform ReadyPanel = GameObject.Find("CameraCanvas").transform.Find("ReadyPanel");
            ReadyPanel.gameObject.SetActive(true);
            ReadyPanel.Find("StartBtn").gameObject.SetActive(false);
            ReadyPanel.Find("CancelBtn").gameObject.SetActive(false);
        }
    }

    [PunRPC]
    void FakeRPC()
    {
        AN.SetTrigger("fake");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(PowerText.text);
            stream.SendNext(SR.flipX);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            PowerText.text = (String)stream.ReceiveNext();
            SR.flipX = (bool)stream.ReceiveNext();
        }

    }
    public void AdjustSpeedAndVision(int numAlive)
    {
        moveSpeed = 10f - numAlive * 0.3f;
        if (PV.IsMine) fieldOfView.SetViewDistance(moveSpeed * 1.5f);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerStatus();
    }

    public void UpdatePlayerStatus()
    {
        if (isStart)
        {
            int min = 20;
            int remainCnt = 0;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                Hashtable table = player.CustomProperties;
                if (!(Convert.ToBoolean(table["dead"])))
                {
                    remainCnt++;
                    if (Convert.ToInt32(table["power"]) < min)
                    {
                        min = Convert.ToInt32(table["power"]);
                        minName = player.NickName;
                    }
                }
            }
            if (min == power)
                isMin = true;
            AdjustSpeedAndVision(remainCnt);
            if (isMin)
            {
                moveSpeed += Math.Max(0, remainCnt - 2) * 0.2f;
                if (PV.IsMine)
                    fieldOfView.SetViewDistance(moveSpeed * 1.5f);
            }
            GameObject.Find("CameraCanvas").transform.Find("InGamePanel").gameObject.SetActive(true);
            GameObject.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("MinText").GetComponent<TMP_Text>().text = "현재 꼴등 : " + minName;
            GameObject.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("RemainText").GetComponent<TMP_Text>().text = remainCnt + "/" + PhotonNetwork.PlayerList.Length;
        }
        else
        {
            moveSpeed = 6f;
            if (PV.IsMine)
                fieldOfView.SetViewDistance(moveSpeed * 1.5f);
        }
        if (isStart && PV.IsMine)
        {
            if (isMin || power == 1)
            {
                minName = NickNameText.text;
                GameObject.Find("CameraCanvas").transform.Find("InGamePanel").gameObject.SetActive(true);
                GameObject.Find("CameraCanvas").transform.Find("InGamePanel").transform.Find("AlramText").gameObject.SetActive(true);
            }
        }
    }
    IEnumerator TeleportRoutine()
    {
        yield return null;
        isControl = false;
        yield return new WaitForSeconds(0.5f);
        transform.position = teleportTarget?.position ?? transform.position;
        yield return new WaitForSeconds(1f);
        isControl = true;

    }
}