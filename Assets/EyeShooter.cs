using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EyeShooter : MonoBehaviour
{
    [Header("Detección")]
    public float detectionRadius = 10f;
    public string playerTag = "Player";

    [Header("Ataque - Proyectiles")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float shootInterval = 2f;

    [Header("Ataque - Rayo Láser")]
    public LineRenderer laserLine;
    public float laserDuration = 2f;
    public float laserDamage = 10f;
    public LayerMask obstacleLayers;
    public LayerMask shieldLayer;

    [Header("Comportamiento")]
    public float switchAttackDelay = 5f;

    Transform currentTarget;
    bool usingLaser = false;
    bool attacking = false;

    void Start()
    {
        StartCoroutine(AttackLoop());
    }

    void Update()
    {
        // buscar el jugador más cercano en tiempo real
        FindClosestTarget();

        // si hay objetivo, rotar hacia él
        if (currentTarget != null)
        {
            Vector2 dir = currentTarget.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void FindClosestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        if (players.Length == 0) { currentTarget = null; return; }

        currentTarget = players
            .OrderBy(p => Vector2.Distance(transform.position, p.transform.position))
            .First().transform;

        // si está fuera del rango, dejar de apuntar
        if (Vector2.Distance(transform.position, currentTarget.position) > detectionRadius)
            currentTarget = null;
    }

    IEnumerator AttackLoop()
    {
        while (true)
        {
            if (currentTarget != null && !attacking)
            {
                attacking = true;

                if (usingLaser)
                    yield return StartCoroutine(FireLaser());
                else
                    yield return StartCoroutine(FireProjectile());

                usingLaser = !usingLaser; // alterna tipo de ataque
                yield return new WaitForSeconds(switchAttackDelay);
                attacking = false;
            }

            yield return null;
        }
    }

    IEnumerator FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || currentTarget == null)
            yield break;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Vector2 dir = (currentTarget.position - firePoint.position).normalized;
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = dir * projectileSpeed;

        yield return new WaitForSeconds(shootInterval);
    }

    IEnumerator FireLaser()
    {
        if (laserLine == null || firePoint == null || currentTarget == null)
            yield break;

        laserLine.enabled = true;
        float elapsed = 0f;

        while (elapsed < laserDuration && currentTarget != null)
        {
            Vector2 dir = (currentTarget.position - firePoint.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, dir, detectionRadius, obstacleLayers | shieldLayer);

            Vector3 endPoint = firePoint.position + (Vector3)(dir * detectionRadius);

            // si el rayo toca algo
            if (hit.collider != null)
            {
                endPoint = hit.point;

                // si toca escudo, se detiene
                if (((1 << hit.collider.gameObject.layer) & shieldLayer) != 0)
                {
                    // puedes poner un efecto visual aquí si quieres
                }
                else
                {
                    // si toca jugador, inflige daño
                    if (hit.collider.CompareTag(playerTag))
                    {
                        // ejemplo: si los jugadores tienen un script "PlayerHealth"
                        //var hp = hit.collider.GetComponent<PlayerHealth>();
                        //if (hp != null)
                            //hp.TakeDamage(laserDamage * Time.deltaTime);
                    }
                }
            }

            laserLine.SetPosition(0, firePoint.position);
            laserLine.SetPosition(1, endPoint);

            elapsed += Time.deltaTime;
            yield return null;
        }

        laserLine.enabled = false;
    }

    // Dibuja el radio de detección en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
