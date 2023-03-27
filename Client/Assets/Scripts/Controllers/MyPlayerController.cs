using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using static Define;
using static UnityEngine.UI.Image;

public class MyPlayerController : PlayerController
{
    AttackRange attackRange;
    CameraController cameraController;
    bool _isControl = true;
    public bool IsControl
    {
        get { return _isControl; }
        set
        {
            if (_isControl == value)
                return;
            _isControl = value;

            // 다른 플레이어 관전에서 내 플레이어 카메라로 돌아옴 
            if (_isControl)
                cameraController.SetTargetPlayer(this);
        }
    }

    int _killCount = 0;
    public int KillCount
    {
        get { return _killCount; }
        set
        {
            if (value < 0)
                return;
            _killCount = value;
        }
    }

    protected override void Start()
    {
        base.Start();

        attackRange = transform.Find("AttackRange").gameObject.GetComponent<AttackRange>();

        cameraController = Camera.main.GetComponent<CameraController>();
        // for MyPlayer target 
        cameraController.SetMyPlayer(this);
        // for 관전 
        cameraController.SetTargetPlayer(this);
    }

    protected override void UpdateController()
    {
        // 공격 범위 위치 세팅 
        if (State != PlayerState.Dead)
            attackRange.SetOrigin(transform.position);

        GetDirInput();
        base.UpdateController();
        GetSkiilInput();
    }

    void GetSkiilInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetIsControl(true);
            // 공격 패킷 연타 부하 방지 
            if (_coAttack != null)
                return;

            C_Attack attack = new C_Attack();
            // 범위 내 적이 있는지 검사 
            Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            foreach (Collider2D collider in collider2Ds)
            {
                // 다른 player를 발견한 경우
                if (collider.tag == "Player" && collider.GetComponent<MyPlayerController>() == null)
                {
                    PlayerController enemy = collider.GetComponent<PlayerController>();

                    PlayerInfo enemyInfo = new PlayerInfo()
                    {
                        PlayerId = enemy.Id,
                        Name = enemy.name,
                        Speed = enemy.Speed,
                        Power = enemy.Power,
                        PosInfo = enemy.PosInfo
                    };
                    attack.Enemys.Add(enemyInfo);
                }
            }    
            
            Managers.Network.Send(attack);

            Attack();
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SetIsControl(true);
            if (_coFake != null)
                return;

            C_Fake fake = new C_Fake();
            Managers.Network.Send(fake);

            Fake();
        }
    }

    void GetDirInput()
    {
        // Left, A -> x = -1
        // Right, D -> x = 1
        // None -> x = 0
        X = Input.GetAxisRaw("Horizontal");
        // Down, S -> y = -1
        // Up, W -> y = 1
        // None -> y = 0
        Y = Input.GetAxisRaw("Vertical");

        if (X == 1 && Y == 1)
        {
            Dir = MoveDir.Upright;
        }
        else if (X == 1 && _y == -1)
        {
            Dir = MoveDir.Downright;
        }
        else if (X == 1 && Y == 0)
        {
            Dir = MoveDir.Right;
        }
        else if (X == -1 && Y == 1)
        {
            Dir = MoveDir.Upleft;
        }
        else if (X == -1 && Y == -1)
        {
            Dir = MoveDir.Downleft;
        }
        else if (X == -1 && Y == 0)
        {
            Dir = MoveDir.Left;
        }
        else if (X == 0 && Y == 1)
        {
            Dir = MoveDir.Up;
        }
        else if (X == 0 && Y == -1)
        {
            Dir = MoveDir.Down;
        }
        else if (X == 0 && Y == 0)
        {
            Dir = MoveDir.Idle;
        }
    }

    protected override void UpdateIsMoving()
    {
        if (Dir == MoveDir.Idle)
        {
            CheckUpdatedFlag();
            return;
        }

        Vector3Int desPos = CellPos;
        switch (Dir)
        {
            case MoveDir.Up:
                desPos += Vector3Int.up;
                break;
            case MoveDir.Upright:
                desPos += Vector3Int.up;
                desPos += Vector3Int.right;
                break;
            case MoveDir.Upleft:
                desPos += Vector3Int.up;
                desPos += Vector3Int.left;
                break;
            case MoveDir.Down:
                desPos += Vector3Int.down;
                break;
            case MoveDir.Downright:
                desPos += Vector3Int.down;
                desPos += Vector3Int.right;
                break;
            case MoveDir.Downleft:
                desPos += Vector3Int.down;
                desPos += Vector3Int.left;
                break;
            case MoveDir.Left:
                desPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                desPos += Vector3Int.right;
                break;
        }

        if (Managers.Map.CanGo(desPos))
        {
            CellPos = desPos;
        }

        // 상태가 변하였다면 이동 패킷 전송 
        CheckUpdatedFlag();
    }

    // 상태가 변하였다면 이동 패킷 전송 
    protected override void CheckUpdatedFlag()
    {
        if (_updated)
        {
            SetIsControl(true);
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }

    public void Kill(int killCount)
    {
        KillCount = killCount;
    }

    public void SetIsControl(bool isControl)
    {
        IsControl = isControl;
    }
}
