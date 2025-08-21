using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPun, IPunObservable, IInRoomCallbacks
{
    public static PlayerMovement instance;

    Rigidbody2D _rb;
    Animator animator;

    public float velocidad;
    public float jumpForce;
    public float squishedGravity;

    float horizontalMovement;

    Vector2 moveVector;

    Vector2 networkPosition;
    float networkRotation;
    float currentTime = 0;
    Color _myColor;
    float currentGravity;

    //Variables que me ayudan a determinar la posición de todos los jugadores de la red
    double currentPacketTime = 0;
    double lastPacketTime = 0;
    [SerializeField]
    float syncSpeed;
    Vector2 positionAtLastPacket = Vector2.zero;
    public float margin = 0.5f; // Ajusta según el tamaño del jugador o deja este valor por defecto

    public float rayDist;
    public LayerMask groundLayer;
    public BoxCollider2D normalCollider;
    public BoxCollider2D squishedCollider;
    private bool isSquished = false;
    private float alturaInicioCaida;
    private bool estaCayendo;
    public float alturaMinimaAplastamiento = 3f;


    private void Awake()
    {
        animator = GetComponent<Animator>();

        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 20;

        _rb = GetComponent<Rigidbody2D>();
        currentGravity = _rb.gravityScale;

        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && pv.Owner != null)
        {
            if (photonView.IsMine)
            {
                instance = this;
                PlayerData.instance.myPlayer = this;
            }

            ApplyColorFromProperties(photonView.Owner);
        }
    }
    private void Start()
    {
        if (CameraController.instance != null)
            CameraController.instance.m_Targets.Add(this.transform);
    }

    [PunRPC]
    public void ApplyColorToPlayer(PlayerMovement player, string colorHex)
    {
        Color color;
        //object colorValue;
        if (ColorUtility.TryParseHtmlString("#" + colorHex, out color))
        {
            player.GetComponent<SpriteRenderer>().color = color;
        }
        //if (photonView.Owner.CustomProperties.TryGetValue("color", out colorValue))
        //{
   
        //}
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            double timeToReachGoal = currentPacketTime - lastPacketTime;
            currentTime += Time.deltaTime;
            transform.position = Vector3.Lerp(positionAtLastPacket, networkPosition, (float)(currentTime / timeToReachGoal) * syncSpeed);
            return;
        }

        horizontalMovement = Input.GetAxisRaw("Horizontal");

        Camera cam = Camera.main;
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        Vector3 camPos = cam.transform.position;

        float minX = camPos.x - horzExtent + margin;
        float maxX = camPos.x + horzExtent - margin;

        Vector3 newPos = transform.position + new Vector3(horizontalMovement * velocidad * Time.deltaTime, 0, 0);
        bool inside = newPos.x > minX && newPos.x < maxX;

        float adjustedSpeed = velocidad;

        if (horizontalMovement != 0)
        {
            bool cameraAtMaxSize = Mathf.Approximately(cam.orthographicSize, CameraController.instance.m_MaxSize);

            if (cameraAtMaxSize)
            {
                if ((horizontalMovement < 0 && transform.position.x <= minX) ||
                    (horizontalMovement > 0 && transform.position.x >= maxX))
                {
                    // Bloquea el movimiento si está en el borde
                    _rb.velocity = new Vector2(0, _rb.velocity.y);
                    return;
                }
            }

            // Movimiento normal dentro de la cámara
            _rb.velocity = new Vector2(horizontalMovement * adjustedSpeed, _rb.velocity.y);
        }
        else
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
        }

        if (photonView.IsMine)
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(_rb.velocity.x));
            }

            if (horizontalMovement != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = horizontalMovement > 0 ? 1f : -1f;
                transform.localScale = scale;
            }

            animator.SetBool("IsJumping", !IsGrounded());

            if (_rb.velocity.y < 0 && !estaCayendo)
            {
                estaCayendo = true;
                alturaInicioCaida = transform.position.y;
            }
            if (_rb.velocity.y >= 0)
            {
                estaCayendo = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!photonView.IsMine) return;

        // Evitar que un jugador ya aplastado pueda volver a ser procesado
        if (isSquished) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement otherMovement = collision.gameObject.GetComponent<PlayerMovement>();
            if (otherMovement != null)
            {
                float myY = transform.position.y;
                float otherY = collision.transform.position.y;

                float diferenciaAlturaCaida = otherMovement.alturaInicioCaida - otherY;

                Debug.Log($"[APLASTAMIENTO] diferencia caída={diferenciaAlturaCaida:F2} (mín {alturaMinimaAplastamiento})");

                // Evitar que el otro esté aplastado también
                if (otherMovement.isSquished) return;

                if (otherY > myY && (otherMovement.alturaInicioCaida - otherY) >= alturaMinimaAplastamiento)
                {
                    Debug.Log("[APLASTAMIENTO] Jugador de arriba aplasta al de abajo");
                    if (IsOverPitOrRendija())
                        photonView.RPC(nameof(RPC_BecomeSquished), RpcTarget.All);
                    else
                        photonView.RPC(nameof(RPC_Respawn), RpcTarget.All);
                }
                else
                {
                    Debug.Log("[APLASTAMIENTO] No alcanzó altura mínima para aplastar");
                }
            }
        }
    }


    [PunRPC]
    private void RPC_BecomeSquished()
    {
        StartCoroutine(BecomeSquished());
    }

    [PunRPC]
    private void RPC_Respawn()
    {
        Respawn();
    }

    private bool IsOverPitOrRendija()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDist,groundLayer);
        bool result = (hit.collider != null);

        return result;
    }

    private IEnumerator BecomeSquished()
    {
        isSquished = true;
        normalCollider.enabled = false;
        squishedCollider.enabled = true;
        _rb.gravityScale = squishedGravity;
        animator.SetBool("IsSquished", true);

        // Estás en modo aplastado. Aquí puedes limitar movimiento si lo deseas.
        yield return new WaitForSeconds(3f); // Ejemplo: se desaplasta en 3 segundos

        animator.SetBool("IsSquished", false);
        squishedCollider.enabled = false;
        _rb.gravityScale = currentGravity;
        normalCollider.enabled = true;
        isSquished = false;
    }

    private void Respawn()
    {
        var currentFlag = RespawnManager.instance.GetCurrentRespawnFlag();
        transform.position = new Vector3(
            currentFlag.respawnPosition.position.x - Random.Range(-currentFlag.respawnDistances, currentFlag.respawnDistances),
            currentFlag.respawnPosition.position.y,
            currentFlag.respawnPosition.position.z
        );
    }

    public void Jump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnDestroy()
    {
        CameraController.instance.m_Targets.Remove(this.transform);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayDist);
    }

    bool IsGrounded()
    {
        if (Physics2D.Raycast(transform.position + new Vector3(0, -0.5f, 0), Vector3.down, 0.4f) ||
            Physics2D.Raycast(transform.position + new Vector3(0.4f, -0.5f, 0), Vector3.down, 0.4f) ||
            Physics2D.Raycast(transform.position + new Vector3(-0.4f, -0.5f, 0), Vector3.down, 0.4f)
            )
            return true;
        else
            return false;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(Mathf.Abs(_rb.velocity.x));
            stream.SendNext(transform.localScale.x);
            stream.SendNext(!IsGrounded());
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            float receivedSpeed = (float)stream.ReceiveNext();
            float receivedScaleX = (float)stream.ReceiveNext();
            bool isJumping = (bool)stream.ReceiveNext();

            currentTime = 0f;
            lastPacketTime = currentPacketTime;
            currentPacketTime = info.SentServerTime;
            positionAtLastPacket = transform.position;

            if (!photonView.IsMine) 
            {
                animator.SetFloat("Speed", receivedSpeed);
                animator.SetBool("IsJumping", isJumping);
                Vector3 scale = transform.localScale;
                scale.x = receivedScaleX;
                transform.localScale = scale;
            }
        }
    }

    bool IsInsideCameraView(Vector3 newPosition)
    {
        Camera cam = Camera.main;
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        Vector3 camPos = cam.transform.position;

        float minX = camPos.x - horzExtent + margin;
        float maxX = camPos.x + horzExtent - margin;

        return newPosition.x > minX && newPosition.x < maxX;
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // Solo reaccionamos si este player es el dueño de este GameObject
        if (targetPlayer == photonView.Owner && changedProps.ContainsKey("color"))
        {
            ApplyColorFromProperties(targetPlayer);
        }
    }

    private void ApplyColorFromProperties(Player player)
    {
        if (player.CustomProperties.TryGetValue("color", out object colorValue))
        {
            string hexColor = colorValue as string;
            if (ColorUtility.TryParseHtmlString("#" + hexColor, out Color color))
            {
                GetComponent<SpriteRenderer>().color = color;
            }
        }
    }

    public void OnPlayerEnteredRoom(Player newPlayer) { }
    public void OnPlayerLeftRoom(Player otherPlayer) { }
    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }
    public void OnMasterClientSwitched(Player newMasterClient) { }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    }
}
