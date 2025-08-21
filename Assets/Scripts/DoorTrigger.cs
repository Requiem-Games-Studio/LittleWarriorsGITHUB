using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorTrigger : MonoBehaviourPun
{
    public Animator doorAnimator;

    private bool isOpen = false;

    public GameObject doorObject;

    private void Awake()
    {
        if (doorAnimator == null && doorObject != null)
            doorAnimator = doorObject.GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (!isOpen)
        {
            photonView.RPC("OpenDoor", RpcTarget.All);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (!isOpen)
        {
            photonView.RPC("OpenDoor", RpcTarget.All);
        }
    }

    [PunRPC]
    void OpenDoor()
    {
        isOpen = true;
        doorAnimator.SetTrigger("Open");
    }
}
