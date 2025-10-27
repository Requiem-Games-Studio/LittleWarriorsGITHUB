using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object has the "Player" tag AND the PlayerMovement component.
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.TryGetComponent(out PlayerMovement playerMovement))
        {
            // If the component exists, you can safely check its 'isHiding' variable.
            if (!playerMovement.isHiding)
            {
               playerMovement.Respawn();
            }
        }
    }
}
