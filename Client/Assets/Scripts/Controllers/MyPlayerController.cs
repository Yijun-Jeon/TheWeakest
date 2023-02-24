using System.Collections;
using System.Collections.Generic;
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
            if (_isAttack == true)
                return;
            _isAttack = true;
            _animator.Play("Bigger");
            _coSkill = StartCoroutine("CoStartBigger");
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _isFake = true;
            _animator.Play("Fake");
            _coSkill = StartCoroutine("CoStartFake");
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
            Dir = MoveDir.UpRight;
        }
        else if (X == 1 && _y == -1)
        {
            Dir = MoveDir.DownRight;
        }
        else if (X == 1 && Y == 0)
        {
            Dir = MoveDir.Right;
        }
        else if (X == -1 && Y == 1)
        {
            Dir = MoveDir.UpLeft;
        }
        else if (X == -1 && Y == -1)
        {
            Dir = MoveDir.DownLeft;
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
}
