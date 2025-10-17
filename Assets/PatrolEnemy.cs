using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 2f;
    public bool startFacingRight = true;

    [Header("Detección")]
    public Transform groundCheck;            // punto en el frente del enemigo, cerca de los pies
    public float groundCheckDistance = 0.2f; // distancia para checar suelo
    public Transform wallCheck;              // punto en el frente para checar muro
    public float wallCheckDistance = 0.2f;   // distancia para checar muro
    public LayerMask groundLayer;            // capas consideradas como suelo/muro

    [Header("Comportamiento")]
    public float flipDelay = 0.15f;          // cuánto se detiene antes de voltearse

    Rigidbody2D rb;
    float currentSpeed;
    int direction = 1; // 1 = derecha, -1 = izquierda
    bool flipping = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = speed;
        direction = startFacingRight ? 1 : -1;

        // Asegurarse que hay puntos asignados
        if (groundCheck == null) Debug.LogWarning("groundCheck no asignado.");
        if (wallCheck == null) Debug.LogWarning("wallCheck no asignado.");
    }

    void FixedUpdate()
    {
        // movimiento horizontal
        rb.velocity = new Vector2(direction * currentSpeed, rb.velocity.y);

        if (!flipping)
        {
            // detectar borde (si no hay suelo justo adelante)
            bool groundAhead = false;
            if (groundCheck != null)
            {
                RaycastHit2D hitGround = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
                groundAhead = hitGround.collider != null;
            }

            // detectar muro delante
            bool wallAhead = false;
            if (wallCheck != null)
            {
                Vector2 dir = transform.right * direction; // dirección local hacia adelante
                RaycastHit2D hitWall = Physics2D.Raycast(wallCheck.position, dir, wallCheckDistance, groundLayer);
                wallAhead = hitWall.collider != null;
            }

            if (!groundAhead || wallAhead)
            {
                StartCoroutine(FlipRoutine());
            }
        }
    }

    IEnumerator FlipRoutine()
    {
        flipping = true;
        float oldSpeed = currentSpeed;
        currentSpeed = 0f;                   // se detiene
        yield return new WaitForSeconds(flipDelay);
        Flip();
        currentSpeed = oldSpeed;             // reanuda velocidad
        flipping = false;
    }

    void Flip()
    {
        direction *= -1;
        // Voltear visualmente: invertir escala X
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (direction == 1 ? 1 : -1);
        transform.localScale = s;
    }

    // Opcional: dibujar raycasts en Scene view para debug
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 dir = Vector3.right * Mathf.Sign(transform.localScale.x);
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + dir * wallCheckDistance);
        }
    }
}
