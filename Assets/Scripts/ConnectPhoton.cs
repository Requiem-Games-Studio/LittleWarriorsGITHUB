using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectPhoton : MonoBehaviourPunCallbacks
{
    //Este script va en la primer escena, se usa para conectarse al photon, te tenes que conectar si o si para poder crear o unirte a salas

    void Start()
    {
        //Se conecta
        PhotonNetwork.ConnectUsingSettings();
    }


    public override void OnConnectedToMaster()
    {
        //Cuando se conecta, se une al lobby
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        //Cuando se une al lobby master, te envia a la siguiente escena, que es donde te podes unir o crear salas
        SceneManager.LoadScene("Lobby");
    }

}
