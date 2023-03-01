using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class PlayerController : MonoBehaviour
{
    public int Id { get; set; }
   
    // 공격 쿨타임 
    protected Coroutine _coAttack;

    // 죽은 척 쿨타임
    protected Coroutine _coFake;

    // dirty flag
    protected bool _updated = false;

    public Grid _grid; // map grid
    float _speed = 10.0f;
    public float Speed
    {
        get { return _speed; }
        set
        {
            if (value < 0)
                return;
            _speed = value;
        }
    }

    PositionInfo _positionInfo = new PositionInfo();
    public PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            if (_positionInfo.Equals(value))
                return;

            CellPos = new Vector3Int(value.PosX, value.PosY);
            State = value.State;
            Dir = value.MoveDir;
            _updated = true;
        }
    }
    
    // 좌표 상의 실제 위치 
    public Vector3Int CellPos
    {
        get { return new Vector3Int(PosInfo.PosX, PosInfo.PosY,0); }
        set
        {
            if (PosInfo.PosX == value.x && PosInfo.PosY == value.y)
                return;
            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            _updated = true;
        }
    }

    // 플레이어 상태 
    public PlayerState State
    {
        get { return PosInfo.State; }
        set
        {
            if (PosInfo.State == value)
                return;

            PosInfo.State = value;
            _updated = true;
        }
    }

    // 플레이어의 방향을 바꿀 때 바로 애니메이션도 같이 처리
    public MoveDir Dir
    {
        get { return PosInfo.MoveDir; }
        set
        {
            if (PosInfo.MoveDir == value)
                return;

            PosInfo.MoveDir = value;
            UpdateLocalScale();
            if (_coAttack == null)
            {
                UpdateAnimation();
            }
            _updated = true;
        }
    }

    // 이동 관련 
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
            _x = value;
        }
    }

    // Down, S -> y = -1
    // Up, W -> y = 1
    // None -> y = 0
    public float Y { get { return _y; } set { _y = value;} }


    protected Animator _animator;
    

    protected virtual void Start()
    {
        _animator = GetComponent<Animator>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.6f);
        transform.position = pos;

        State = PlayerState.Alive;
        Dir = MoveDir.Idle;
        UpdateLocalScale();
    }
    
    void Update()
    {
        if(_coFake == null)
            UpdateController();
    }

    void UpdateAnimation()
    {
        if (Dir == MoveDir.Idle)
        {
            _animator.Play("Idle");
        }
        else
        {
            _animator.Play("Walk");
        }
    }

    protected virtual void UpdateController()
    {
        UpdatePosition();
    }

    // 실제로 스르르 이동 
    void UpdatePosition()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.6f);
        // 방향 vector - 2가지의 정보 : 실제 이동하는 방향, 이동하려는 목적지까지의 크기
        Vector3 moveDir = destPos - transform.position;

        // 도착 여부 체크
        float dist = moveDir.magnitude;
        if (dist < _speed * Time.deltaTime)
        {
            transform.position = destPos;
            UpdateIsMoving();
        }
        else
        {
            // 스르르 움직이게 처리
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
        }
    }

    
    // 이동 가능한 상태일 때 실제 좌표 이동
    protected virtual void UpdateIsMoving()
    {
    }

    // 좌우 방향에 따라 플레이어 대칭 처리 
    void UpdateLocalScale()
    {
        switch (Dir)
        {
            case MoveDir.Upright:
            case MoveDir.Downright:
            case MoveDir.Right:
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                transform.GetComponentInChildren<Canvas>().transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case MoveDir.Upleft:
            case MoveDir.Downleft:
            case MoveDir.Left:
                transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                transform.GetComponentInChildren<Canvas>().transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                break;
        }
    }

    public void Attack()
    {
        _coAttack = StartCoroutine("CoStartAttack");
    }

    public void Fake()
    {
        _coFake = StartCoroutine("CoStartFake");
    }


    IEnumerator CoStartAttack()
    {
        Dir = MoveDir.Idle;
        _animator.Play("Bigger");
        // TODO : 피격 판정

        yield return new WaitForSeconds(1.2f);
        if (_coFake == null)
        {
            UpdateAnimation();
        }
        _coAttack = null;

        // Dir 변화 전달 
        CheckUpdatedFlag();
    }

    IEnumerator CoStartFake()
    {
        Dir = MoveDir.Idle;
        _animator.Play("Fake");
        // TODO : 컨트롤 제한? 

        yield return new WaitForSeconds(5f);

        _coAttack = null;
        _coFake = null;

        UpdateAnimation();

        // Dir 변화 전달 
        CheckUpdatedFlag();
    }

    // 초기 접속시 위치 싱크 
    public void SyncsPos()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.6f);
        transform.position = destPos;
    }

    protected virtual void CheckUpdatedFlag()
    {

    }
}
