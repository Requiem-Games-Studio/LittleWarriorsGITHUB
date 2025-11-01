using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MasterClientOptions : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private const byte EVENT_END_GAME = 1;

    public GameObject resetRoomBTN;
    public GameObject exitRoomBTN;
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Update()
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
            var currentScene = SceneManager.GetActiveScene();
            PhotonNetwork.LoadLevel(currentScene.buildIndex); // Debe estar en el build settings
        }
    }

    [PunRPC]
    public void RPC_ResetScene()
    {
        var currentScene = SceneManager.GetActiveScene();
        PhotonNetwork.LoadLevel(currentScene.buildIndex); // Debe estar en el build settings
    }


    private IEnumerator LeaveRoomAndReturnToLobby(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.LeaveRoom();
    }
    public void EndGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master envía evento END_GAME a todos.");

            PhotonNetwork.RaiseEvent(
                EVENT_END_GAME,
                null,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                ExitGames.Client.Photon.SendOptions.SendReliable
            );
        }
        else
        {
            Debug.Log("No soy master, no puedo enviar evento.");
        }
    }
    // Este método se ejecuta en todos los clientes cuando llega el evento
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == EVENT_END_GAME)
        {
            StartCoroutine(LeaveRoomAndReturnToLobby());
        }
    }

    private IEnumerator LeaveRoomAndReturnToLobby()
    {
        yield return new WaitForSeconds(0.1f); // Pequeño delay por seguridad

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public void ExitGame()
    {
        PhotonNetwork.LeaveRoom();
    }


    // Este método se llama automáticamente cuando el cliente termina de salir de la sala
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("Lobby");
    }



}
