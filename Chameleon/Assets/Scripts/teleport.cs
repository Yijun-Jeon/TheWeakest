using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class teleport : MonoBehaviour
{
    public GameObject targetObj;
    public GameObject toObj;
    Animator animator;
    public int playerCount;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerCount = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            targetObj = collision.gameObject;
            targetObj.GetComponent<PlayerScript>().teleportTarget = toObj.transform;
            targetObj.GetComponent<PlayerScript>().canTeleport = true;
            playerCount++;
        }
        if (playerCount > 0)
            animator.SetBool("gate", true);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            targetObj = collision.gameObject;
            targetObj.GetComponent<PlayerScript>().teleportTarget = null;
            targetObj.GetComponent<PlayerScript>().canTeleport = false;
            playerCount--;
        }
        if (playerCount <= 0)
            animator.SetBool("gate", false);
    }
    // IEnumerator TeleportRoutine()
    // {
    //     yield return null;
    //     targetObj.GetComponent<PlayerScript>().isControl = false;
    //     yield return new WaitForSeconds(0.5f);
    //     targetObj.transform.position = toObj.transform.position;
    //     yield return new WaitForSeconds(1f);
    //     targetObj.GetComponent<PlayerScript>().isControl = true;

    // }
}
