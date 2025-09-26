using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class ColorSelector : MonoBehaviour
{
    public List<SkinMaker> skins;
    public List<GameObject> colorsSelected;
    public int colorSelected = 0;
    public SkinMaker skinSelected;


    private void Awake()
    {
        if (PlayerData.instance != null)
        {
            colorSelected = PlayerData.instance.colorNumber;
            skinSelected = skins[6]; //Skin default
        }

        colorsSelected[colorSelected].SetActive(true);

        SelectColor(colorSelected);
    }

    public void SelectColor(int index)
    {
        colorSelected = index;

        if (PlayerData.instance != null)
        {
            var skin = SkinsManager.instance.GetSkinValue(index);
            PlayerData.instance.mySkin = skin;
            skinSelected = skin;
            PlayerData.instance.colorNumber = index;
        }

        if (PlayerData.instance.myPlayer != null)
        {
            PlayerData.instance.myPlayer.GetComponent<PlayerSkin>().SetSkinLocal();
        }

        foreach (var color in colorsSelected)
        {
            color.SetActive(false);
        }

        colorsSelected[index].SetActive(true);

        //Hashtable playerProperties = new Hashtable();
        //var colorHex = ColorUtility.ToHtmlStringRGB(colors[index]);
        //playerProperties["color"] = colorHex; // o puedes guardar un int o código RGB
        //PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        //PlayerData.instance.myPlayer.ApplyColorFromProperties(PlayerData.instance.myPlayer);
    }
}
