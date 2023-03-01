using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    MyPlayerController _myPlayer;

    Vector3 offset = new Vector3(0f, 0f, -10f);
    float smoothTime = 0.25f;
    Vector3 velocity = Vector3.zero;

    private void Start()
    {
    }

    private void Update()
    {
        if (_myPlayer == null)
            return;

        Vector3 cameraPosition = new Vector3(_myPlayer.transform.position.x, _myPlayer.transform.position.y, 0) + offset;
        transform.position = Vector3.SmoothDamp(transform.position, cameraPosition, ref velocity, smoothTime);
    }

    public void SetMyPlayer(MyPlayerController myPlayer)
    {
        _myPlayer = myPlayer;
    }
}