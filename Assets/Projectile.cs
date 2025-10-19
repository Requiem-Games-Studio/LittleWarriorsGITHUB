using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 3f;
    public float damage = 10f;
    public LayerMask targetLayers;

    void Start()
    {
        Destroy(gameObject, lifetime); // se destruye solo después de un tiempo
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // verificar si el objeto está en la capa de objetivo
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            // si tiene vida (por ejemplo un script PlayerHealth)
            //var hp = other.GetComponent<PlayerHealth>();
            //if (hp != null)
            //hp.TakeDamage(damage);

            Destroy(gameObject); // destruir el proyectil al impactar
        }

        // si toca un muro o escudo, también se destruye
        if (other.CompareTag("Wall") || other.CompareTag("Shield"))
            Destroy(gameObject);
    }
}
