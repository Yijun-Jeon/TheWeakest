using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private FieldOfView fieldOfView;
    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;
    public Rigidbody2D targetRB = null;
    public Transform targetTF = null;


    private void Update()
    {
        if (targetRB != null)
        {
            Vector3 targetRBPosition = new Vector3(targetRB.position.x, targetRB.position.y, 0) + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetRBPosition, ref velocity, smoothTime);
            fieldOfView.SetOrigin(targetTF.position);
        }
    }
}