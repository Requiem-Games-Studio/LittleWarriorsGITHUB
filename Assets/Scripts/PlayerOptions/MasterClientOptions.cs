using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MasterClientOptions : MonoBehaviourPunCallbacks
{
    public GameObject resetRoomBTN;
    public GameObject exitRoomBTN;
    private void Awake()
    {

        if (PhotonNetwork.IsMasterClient)
        {
            resetRoomBTN.GetComponent<Button>().interactable = true;
            exitRoomBTN.GetComponent<Button>().interactable = true;
        }
        else
        {
            resetRoomBTN.GetComponent<Button>().interactable = false;
            exitRoomBTN.GetComponent<Button>().interactable = false;
        }
    }

    public void ResetRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_ResetScene", RpcTarget.Others);
            PhotonNetwork.LoadLevel("Game"); // Debe estar en el build settings
        }
    }

    [PunRPC]
    public void RPC_ResetScene()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    [PunRPC]
    public void RPC_EndGame()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void EndGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_EndGame", RpcTarget.All);
        }
    }

    // Este método se llama automáticamente cuando el cliente termina de salir de la sala
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("Lobby");
    }
}
