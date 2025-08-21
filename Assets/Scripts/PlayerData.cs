using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    public PlayerMovement myPlayer;
    public int colorNumber;
    Color myColor;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void SetColor(Color color)
    {
        myColor = color;
    }

    public Color GetColor()
    {
        return myColor;
    }
}
