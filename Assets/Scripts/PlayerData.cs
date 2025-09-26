using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    public PlayerMovement myPlayer;
    public int colorNumber;
    Color myColor;

    public SkinMaker mySkin;


    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void SetColor(SkinMaker skin)
    {
        mySkin = skin;
    }

    public Color GetColor()
    {
        return myColor;
    }
}
