using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
    protected override void UpdateController()
    {
        GetDirInput();
        base.UpdateController();
        GetSkiilInput();
    }

    // 카메라 제어의 경우 LateUpdate에서 주로 설정
    void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    void GetSkiilInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 공격 패킷 연타 부하 방지 
            if (_coAttack != null)
                return;

            C_Attack attack = new C_Attack();
            Managers.Network.Send(attack);

            Attack();
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
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
        if(Dir == MoveDir.Idle)
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
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }
}
