using Photon.Pun;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public AudioClip spawnSound;
    private void Start()
    {
        Debug.Log("[GameManager] Start ejecutado, limpiando TagObject");

        // Forzar limpieza para que se vuelva a spawnear siempre que recargue la escena
        PhotonNetwork.LocalPlayer.TagObject = null;

        if (PhotonNetwork.LocalPlayer.TagObject == null)
        {
            StartCoroutine(SpawnPlayerWithDelay());
        }
    }

    IEnumerator SpawnPlayerWithDelay()
    {
        yield return new WaitForSeconds(0.2f); // Delay por seguridad

        Vector3 spawnPos = GetSpawnPosition();

        GameObject player = PhotonNetwork.Instantiate("Player", spawnPos, Quaternion.identity);
        SoundManager.instance.PlaySound(spawnSound);
        // Guardamos una referencia para prevenir futuros spawns duplicados
        PhotonNetwork.LocalPlayer.TagObject = player;
    }

    private Vector3 GetSpawnPosition()
    {
        var currentFlag = RespawnManager.instance.currentRespawnFlag;

        // Verificación de seguridad por si el flag aún no está seteado
        if (currentFlag == null)
        {
            Debug.LogWarning("No se encontró un flag de respawn, usando posición (0,0,0) por defecto.");
            return Vector3.zero;
        }

        float randomX = Random.Range(-currentFlag.respawnDistances, currentFlag.respawnDistances);
        return new Vector3(
            currentFlag.respawnPosition.position.x + randomX,
            currentFlag.respawnPosition.position.y,
            currentFlag.respawnPosition.position.z
        );
    }
}