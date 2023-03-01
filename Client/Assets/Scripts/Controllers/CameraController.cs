using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private FieldOfView fieldOfView;

    private Vector3 offset = new Vector3(0f, 0f, -10f);
    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        fieldOfView = GameObject.Find("FieldOfView").GetComponent<FieldOfView>();
        Debug.Log("Camera Start");
    }

    private void Update()
    {
        fieldOfView.SetOrigin(this.transform.position);
        Debug.Log("Camera Update");
    }
}