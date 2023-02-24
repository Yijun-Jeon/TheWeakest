using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class PlayerController : MonoBehaviour
{
    public int Id { get; set; }

    public Grid _grid; // map grid
    
    // 좌표 상의 실제 위치 
    public Vector3Int CellPos { get; set; } = new Vector3Int(0, 0, 0);
    MoveDir _dir = MoveDir.Idle;
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

    protected float _x = 1;
    protected float _y = 0;

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


    protected Animator _animator;
    // 플레이어의 방향을 바꿀 때 바로 애니메이션도 같이 처리
    public MoveDir Dir
    {
        get { return _dir; }
        set
        {
            if (_dir == value)
                return;
            _dir = value;
            if(_isAttack)
            {
                return;
            }
            if (value == MoveDir.Idle)
            {
                _animator.Play("Idle");
            }
            else
            {
                _animator.Play("Walk");
            }
        }
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.6f);
        transform.position = pos;
    }
    
    void Update()
    {
        if(_isFake == false)
            UpdateController();
    }

    protected virtual void UpdateController()
    {
        UpdatePosition();
        UpdateIsMoving();
    }

    protected Coroutine _coSkill;
    protected bool _isAttack = false;
    protected bool _isFake = false;


    // 실제로 스르르 이동 
    void UpdatePosition()
    {
        if (_isMoving == false)
            return;

        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.6f);
        // 방향 vector - 2가지의 정보 : 실제 이동하는 방향, 이동하려는 목적지까지의 크기
        Vector3 moveDir = destPos - transform.position;

        // 도착 여부 체크
        float dist = moveDir.magnitude;
        if (dist < _speed * Time.deltaTime)
        {
            transform.position = destPos;
            _isMoving = false;
        }
        else
        {
            // 스르르 움직이게 처리
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            _isMoving = true;
        }
    }

    
    // 이동 가능한 상태일 때 실제 좌표 이동
    void UpdateIsMoving()
    {
        if(_isMoving == false && Dir != MoveDir.Idle)
        {
            Vector3Int desPos = CellPos;
            switch (_dir)
            {
                case MoveDir.Up:
                    desPos += Vector3Int.up;
                    break;
                case MoveDir.UpRight:
                    desPos += Vector3Int.up;
                    desPos += Vector3Int.right;
                    break;
                case MoveDir.UpLeft:
                    desPos += Vector3Int.up;
                    desPos += Vector3Int.left;
                    break;
                case MoveDir.Down:
                    desPos += Vector3Int.down;
                    break;
                case MoveDir.DownRight:
                    desPos += Vector3Int.down;
                    desPos += Vector3Int.right;
                    break;
                case MoveDir.DownLeft:
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

            if(Managers.Map.CanGo(desPos))
            {
                CellPos = desPos;
                _isMoving = true;
            }
        }
    }

    IEnumerator CoStartBigger()
    {
        // TODO : 피격 판정

        yield return new WaitForSeconds(1.2f);
        if (_isFake == false)
        {
            if (_isMoving == true)
                _animator.Play("Walk");
            else
                _animator.Play("Idle");
        }
        _coSkill = null;
        _isAttack = false;
    }

    IEnumerator CoStartFake()
    {
        // TODO : 컨트롤 제한

        yield return new WaitForSeconds(5f);
        _animator.Play("Idle");
        _coSkill = null;
        _isFake = false;
    }
}
