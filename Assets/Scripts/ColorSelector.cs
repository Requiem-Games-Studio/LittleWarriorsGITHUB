using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class ColorSelector : MonoBehaviour
{
    public List<Color> colors;
    public List<GameObject> colorsSelected;
    public int colorSelected = 0;


    private void Awake()
    {
        if (PlayerData.instance != null)
            colorSelected = PlayerData.instance.colorNumber;

        colorsSelected[colorSelected].SetActive(true);

        SelectColor(colorSelected);
    }

    public void SelectColor(int index)
    {
        colorSelected = index;

        if (PlayerData.instance != null)
            PlayerData.instance.colorNumber = index;

        foreach (var color in colorsSelected)
        {
            color.SetActive(false);
        }

        colorsSelected[index].SetActive(true);

        Hashtable playerProperties = new Hashtable();
        var colorHex = ColorUtility.ToHtmlStringRGB(colors[index]);
        playerProperties["color"] = colorHex; // o puedes guardar un int o código RGB
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        //PlayerData.instance.myPlayer.ApplyColorFromProperties(PlayerData.instance.myPlayer);
    }
}
