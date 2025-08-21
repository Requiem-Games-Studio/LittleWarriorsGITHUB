using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnFlag : MonoBehaviour
{
    //Variación de respawns por si caen todos al mismo tiempo, que respawnen a cierta distancia X del respawn, por ejemplo si elijo 3, puede spawnear desde -3 a 3, según el x del respawn
    public float respawnDistances;
    public Transform respawnPosition;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            RespawnManager.instance.SetRespawnFlag(this);
        }
    }
}
