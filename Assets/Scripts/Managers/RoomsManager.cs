using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomsManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    TMP_InputField createRoomInput;
    [SerializeField]
    TMP_InputField joinRoomInput;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }


    public void CreateRoom()
    {
        //Creo una sala con el nombre que el jugador pone en el Input
        PhotonNetwork.CreateRoom(createRoomInput.text);
    }
    public void ConnectToRoom()
    {
        //Me una sala con el nombre que el jugador pone en el Input
        PhotonNetwork.JoinRoom(joinRoomInput.text);
    }

    public override void OnJoinedRoom()
    {
        //Cuando creo o me conecto correctamente a una sala, cambio la escena al juego
        PhotonNetwork.LoadLevel("Game2");
    }
}
