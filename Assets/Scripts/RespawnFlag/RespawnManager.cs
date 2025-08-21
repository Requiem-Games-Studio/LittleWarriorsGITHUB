using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager instance;
    public RespawnFlag firstRespawnFlag;
    public RespawnFlag currentRespawnFlag;

    private void Awake()
    {
        instance = this;
        if(firstRespawnFlag != null)
        {
            currentRespawnFlag = firstRespawnFlag;
        }
    }

    public void SetRespawnFlag(RespawnFlag respawnFlag)
    {
        currentRespawnFlag = respawnFlag;
    }

    public RespawnFlag GetCurrentRespawnFlag()
    {
        if (currentRespawnFlag != null)
            return currentRespawnFlag;
        else return null;
    }
}
