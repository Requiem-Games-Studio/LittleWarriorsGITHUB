using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkin : MonoBehaviourPunCallbacks
{
    //BODY PARTS TO SET
    public SpriteRenderer OJODERECHO;
    public SpriteRenderer OJOIZQUIERDO;
    public SpriteRenderer BRAZODERECHO;
    public SpriteRenderer BRAZOIZQUIERDO;
    public SpriteRenderer CUERPO;
    public SpriteRenderer PIEDERECHO;
    public SpriteRenderer PIEIZQUIERDO;

    public SkinMaker mySkin;

    private void Start()
    {
        SetSkinLocal();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        // Cuando un nuevo jugador entra, le sincronizas su skin con los otros jugadores
        int skinID = SkinsManager.instance.GetSkinID(mySkin);
        photonView.RPC("SyncPlayerSkin", RpcTarget.Others, photonView.Owner.ActorNumber, skinID);
    }

    public void SetSkinLocal()
    {
        if (!photonView.IsMine)
            return;

        mySkin = PlayerData.instance.mySkin;
        int skinID = SkinsManager.instance.GetSkinID(mySkin);
        //mySkin = SkinsManager.instance.GetSkinValue(skinID);

        // Aplica skin localmente
        ApplySkinLocal(mySkin);

        // Notifica a los demás jugadores
        photonView.RPC("ApplySkinRemote", RpcTarget.Others, skinID);
    }

    private void ApplySkinLocal(SkinMaker skin)
    {
        OJODERECHO.sprite = skin.ojoDerecho;
        OJOIZQUIERDO.sprite = skin.ojoIzquierdo;

        BRAZODERECHO.sprite = skin.brazoDerecho;
        BRAZOIZQUIERDO.sprite = skin.brazoIzquierdo;

        CUERPO.sprite = skin.cuerpo;

        PIEDERECHO.sprite = skin.pieDerecho;
        PIEIZQUIERDO.sprite = skin.pieIzquierdo;
    }

    [PunRPC]
    public void ApplySkinRemote(int skinID)
    {
        SkinMaker skin = SkinsManager.instance.GetSkinValue(skinID);
        ApplySkinLocal(skin);
    }


    [PunRPC]
    public void ApplySkinServer(PlayerSkin player, SkinMaker skin)
    {
        player.OJODERECHO.sprite = skin.ojoDerecho;
        player.OJOIZQUIERDO.sprite = skin.ojoIzquierdo;

        player.BRAZODERECHO.sprite = skin.brazoDerecho;
        player.BRAZOIZQUIERDO.sprite = skin.brazoIzquierdo;

        player.CUERPO.sprite = skin.cuerpo;


        player.PIEDERECHO.sprite = skin.pieDerecho;
        player.PIEIZQUIERDO.sprite = skin.pieIzquierdo;
    }

    [PunRPC]
    public void SyncPlayerSkin(int playerActorNumber, int skinID)
    {
        // Verifica que este sea el jugador que se conecta
        if (photonView.Owner.ActorNumber == playerActorNumber)
        {
            SkinMaker skin = SkinsManager.instance.GetSkinValue(skinID);
            ApplySkinLocal(skin);
        }
    }
}
