using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class PlayerController : MonoBehaviour
{
    public Grid _grid; // map grid
    
    // 좌표 상의 실제 위치 
    Vector3Int _cellPos = new Vector3Int(0, 0, 0);
    MoveDir _dir = MoveDir.Down;
    bool _isMoving = false;

    float _speed = 10.0f;
    public  float Speed
    {
        get { return _speed; }
        set
        {
            if (value < 0)
                return;
            _speed = value;
        }
    }

    Vector3 moveDirection;

    float _x = 1;
    float _y = 0;

    public float X
    {
        // Left, A -> x = -1
        // Right, D -> x = 1
        // None -> x = 0
        get { return _x; }
        set
        {
            if (X == value)
                return;
            // 좌우 방향에 따라 플레이어 대칭 처리 
            switch (value)
            {
                case 1:
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    transform.GetComponentInChildren<Canvas>().transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                case -1:
                    transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                    transform.GetComponentInChildren<Canvas>().transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                    break;
            }
            _x = value;
        }
    }

    // Down, S -> y = -1
    // Up, W -> y = 1
    // None -> y = 0
    public float Y { get { return _y; } set { _y = value; } }


    Animator _animator;
    // 플레이어의 방향을 바꿀 때 바로 애니메이션도 같이 처리
    public MoveDir Dir
    {
        get { return _dir; }
        set
        {
            if (_dir == value)
                return;
            if(value == MoveDir.Idle)
            {
                _animator.Play("Idle");
            }
            else
            {
                _animator.Play("Walk");
            }
            _dir = value;
        }
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        Vector3 pos = _grid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.6f);
        transform.position = pos;
    }
    
    void Update()
    {
        GetDirInput();
        UpdatePosition();
        UpdateIsMoving();
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

        if(X == 1 && Y == 1)
        {
            Dir = MoveDir.UpRight;
        }
        else if(X == 1 && _y == -1)
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

    // 실제로 스르르 이동 
    void UpdatePosition()
    {
        if (_isMoving == false)
            return;


        // 방향 Vector - 2가지의 정보 : 실제 이동하는 방향, 이동하려는 목적지까지의 크기
        moveDirection = new Vector3(X, Y, 0);

        Vector3 destPos = transform.position + moveDirection;

        //도착 여부 체크
        float dist = moveDirection.magnitude;
        if (dist < _speed * Time.deltaTime)
        {
            // 도착함 
            transform.position = destPos;
            _isMoving = false;
        }
        else
        {
            // 스르르 움직이게 처리
            // 기기마다 Frame이 다를 수 있기 때문에 deltaTime을 곱해서 모든 머신에서 같게 보이게 함 
            transform.position += moveDirection * Speed * Time.deltaTime;
            _isMoving = true;
        }
    }

    
    // 이동 가능한 상태일 때 실제 좌표 이동
    void UpdateIsMoving()
    {

        if(_isMoving == false)
        {
            switch (_dir)
            {
                case MoveDir.Up:
                    _cellPos += Vector3Int.down;
                    _isMoving = true;
                    break;
                case MoveDir.UpRight:
                    _cellPos += Vector3Int.down;
                    _cellPos += Vector3Int.right;
                    _isMoving = true;
                    break;
                case MoveDir.UpLeft:
                    _cellPos += Vector3Int.down;
                    _cellPos += Vector3Int.left;
                    _isMoving = true;
                    break;
                case MoveDir.Down:
                    _cellPos += Vector3Int.down;
                    _isMoving = true;
                    break;
                case MoveDir.DownRight:
                    _cellPos += Vector3Int.down;
                    _cellPos += Vector3Int.right;
                    _isMoving = true;
                    break;
                case MoveDir.DownLeft:
                    _cellPos += Vector3Int.down;
                    _cellPos += Vector3Int.left;
                    _isMoving = true;
                    break;
                case MoveDir.Left:
                    _cellPos += Vector3Int.left;
                    _isMoving = true;
                    break;
                case MoveDir.Right:
                    _cellPos += Vector3Int.right;
                    _isMoving = true;
                    break;
            }
        }
    }
}
