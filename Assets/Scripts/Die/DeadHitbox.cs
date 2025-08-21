using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var currentFlag = RespawnManager.instance.GetCurrentRespawnFlag();
            collision.transform.position = new Vector3(currentFlag.respawnPosition.position.x - Random.Range(-currentFlag.respawnDistances, currentFlag.respawnDistances), currentFlag.respawnPosition.position.y, currentFlag.respawnPosition.position.z);
        }
    }
}
