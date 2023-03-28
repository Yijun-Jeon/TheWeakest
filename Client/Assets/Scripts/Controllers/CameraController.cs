using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    MyPlayerController _myPlayer;
    public PlayerController TargetPlayer;
    FieldOfView fieldOfView;


    Vector3 offset = new Vector3(0f, 0f, -10f);
    float smoothTime = 0.25f;
    Vector3 velocity = Vector3.zero;

    private void Start()
    {
    }

    public void SetFieldOfView()
    {
        fieldOfView = GameObject.Find("FieldOfView").GetComponent<FieldOfView>();
    }

    private void Update()
    {
        if (TargetPlayer == null)
            return;

        Vector3 cameraPosition = new Vector3(TargetPlayer.transform.position.x, TargetPlayer.transform.position.y, 0) + offset;
        transform.position = Vector3.SmoothDamp(transform.position, cameraPosition, ref velocity, smoothTime);
        fieldOfView.SetOrigin(TargetPlayer.transform.position);
    }

    public void SetMyPlayer(MyPlayerController myPlayer)
    {
        _myPlayer = myPlayer;
    }

    public void SetTargetPlayer(PlayerController player)
    {
        TargetPlayer = player;
        if (player.Id == _myPlayer.Id)
            _myPlayer.SetIsControl(true);
        else
            _myPlayer.SetIsControl(false);
    }

    public void UpdateViewDistance(float speed)
    {
        fieldOfView.SetViewDistance(speed * 1.5f);
    }
}