/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class AttackRange : MonoBehaviour
{

    [SerializeField] private LayerMask layerMask;
    private Mesh mesh;
    [Range(0, 360)]
    [SerializeField] private float fov;
    [SerializeField] private float viewDistance;
    private Vector3 origin;
    private float startingAngle = 0f;
    private Color baseColor;
    private Color activatedColor;
    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        baseColor = GetComponent<MeshRenderer>().material.color;
        activatedColor = GetComponent<MeshRenderer>().material.color;
        activatedColor.a += 0.2f;

        // fov = 180f;
        // viewDistance = 2f;
        origin = Vector3.zero;
    }

    private void LateUpdate()
    {
        int rayCount = 500;
        float angle = startingAngle;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, UtilsClass.GetVectorFromAngle(angle), viewDistance, layerMask);
            if (raycastHit2D.collider == null)
            {
                // No hit
                vertex = UtilsClass.GetVectorFromAngle(angle) * viewDistance;
            }
            else
            {
                // Hit object
                vertex = raycastHit2D.point - new Vector2(origin.x, origin.y);
            }
            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }


        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(origin, Vector3.one * 1000f);

        // ?????? ??? ?????? ????????? ?????? 
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(origin, 1.5f);
        bool activated = false;
        foreach(Collider2D collider in collider2Ds)
        {
            // player??? ????????? ??????
            if(collider.tag == "Player" && collider.GetComponent<MyPlayerController>() == null)
            {
                activated = true;
                break;
            }
        }
        SetColor(activated);
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection)
    {
        startingAngle = UtilsClass.GetAngleFromVectorFloat(aimDirection) + fov / 2f;
    }

    public void SetFoV(float fov)
    {
        this.fov = fov;
    }

    public void SetViewDistance(float viewDistance)
    {
        this.viewDistance = viewDistance;
    }

    public void SetColor(bool activated)
    {
        GetComponent<MeshRenderer>().material.color = activated ? activatedColor : baseColor;
    }
}