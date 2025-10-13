using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviourPun, IPunObservable, IInRoomCallbacks
{
    public static PlayerMovement instance;

    Rigidbody2D _rb;
    public Animator animator;

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
    private float tiempoUltimaCaida;
    public bool cayendoRecientemente => Time.time - tiempoUltimaCaida < 0.2f; // cayó en los últimos 0.2s

    [Header("Extremidades para apagarlas")]
    public GameObject brazoIzquierda;
    public GameObject brazoDerecha;
    public GameObject pieIzquierda;
    public GameObject pieDerecha;


    private void Awake()
    {

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

            //ApplyColorFromProperties(photonView.Owner);
        }
    }
    private void Start()
    {
        if (CameraController.instance != null)
            CameraController.instance.m_Targets.Add(this.transform);
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

        if (isSquished)
        {
            HandleSquishedMovement();
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

        if (horizontalMovement != 0)
        {
            bool cameraAtMaxSize = Mathf.Approximately(cam.orthographicSize, CameraController.instance.m_MaxSize);

            if (cameraAtMaxSize)
            {
                if ((horizontalMovement < 0 && transform.position.x <= minX) ||
                    (horizontalMovement > 0 && transform.position.x >= maxX))
                {
                    _rb.velocity = new Vector2(0, _rb.velocity.y);
                    return;
                }
            }

            _rb.velocity = new Vector2(horizontalMovement * velocidad, _rb.velocity.y);
        }
        else
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(_rb.velocity.x));
            animator.SetBool("IsJumping", !IsGrounded());
        }

        if (horizontalMovement != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = horizontalMovement > 0 ? 1f : -1f;
            transform.localScale = scale;
        }

        if (_rb.velocity.y < -0.2f)
        {
            if (!estaCayendo)
            {
                estaCayendo = true;
                alturaInicioCaida = transform.position.y;
                photonView.RPC(nameof(RPC_UpdateAlturaInicioCaida), RpcTarget.Others, alturaInicioCaida);
                Debug.Log($"[CAIDA] Empieza caída desde {alturaInicioCaida:F2}");
            }
        }
        else if (_rb.velocity.y >= -0.1f && estaCayendo)
        {
            estaCayendo = false;
            Debug.Log($"[CAIDA] Termina caída (última altura={transform.position.y:F2})");
        }

        // --- SALTO ---
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }
    }

    void HandleSquishedMovement()
    {
        _rb.gravityScale = 0f;
        float verticalMovement = Input.GetAxisRaw("Vertical");
        float squishedSpeed = 3f;
        _rb.velocity = new Vector2(0f, verticalMovement * squishedSpeed);
    }

    // --- DETECCIÓN DE APLASTAMIENTO ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!photonView.IsMine || isSquished) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement other = collision.gameObject.GetComponent<PlayerMovement>();
            if (other == null || other.isSquished) return;

            float myY = transform.position.y;
            float otherY = collision.transform.position.y;

            if (otherY <= myY + 0.2f) return;

            float diferenciaAlturaCaida = Mathf.Abs(otherY - other.alturaInicioCaida);

            bool caidaSuficiente = diferenciaAlturaCaida >= alturaMinimaAplastamiento;
            bool sobreRendija = IsOverPitOrRendija();

            if (caidaSuficiente && sobreRendija)
            {
                Rendija rendija = GetRendija();
                transform.DOMove(rendija.transform.position, 0.75f);
                photonView.RPC(nameof(RPC_BecomeSquished), RpcTarget.All);
            }
            else
            {
                photonView.RPC(nameof(RPC_Respawn), RpcTarget.All);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return; //Si salgo de una rendija

        if (collision.gameObject.GetComponent<RendijaExit>())
        {
            if (isSquished)
            {
                transform.DOMove(collision.gameObject.GetComponent<RendijaExit>().exitPosition.position, 0.75f);
                photonView.RPC(nameof(RPC_ExitSquished), RpcTarget.All);
            }
        }
    }


    // --- RPC ---
    [PunRPC]
    void RPC_UpdateAlturaInicioCaida(float nuevaAltura)
    {
        alturaInicioCaida = nuevaAltura;
    }

    [PunRPC]
    private void RPC_BecomeSquished()
    {
        StartCoroutine(EnterSquishedMode());

    }

    [PunRPC]
    private void RPC_ExitSquished()
    {
        StartCoroutine(ExitSquishedMode());
    }

    [PunRPC]
    private void RPC_Respawn()
    {
        Respawn();
    }

    private bool IsOverPitOrRendija()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDist, groundLayer);
        bool result = (hit.collider != null);

        return result;
    }

    private Rendija GetRendija()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDist, groundLayer);
        return hit.collider.gameObject.GetComponent<Rendija>();
    }

    public IEnumerator EnterSquishedMode()
    {
        animator.SetBool("IsSquished", true);

        // Apagar extremidades
        brazoDerecha.SetActive(false);
        brazoIzquierda.SetActive(false);
        pieIzquierda.SetActive(false);
        pieDerecha.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        isSquished = true;
        normalCollider.enabled = false;
        squishedCollider.enabled = true;
        _rb.gravityScale = squishedGravity;

    }

    public IEnumerator ExitSquishedMode()
    {
        animator.SetBool("IsSquished", false);

        // Restaurar extremidades
        brazoDerecha.SetActive(true);
        brazoIzquierda.SetActive(true);
        pieIzquierda.SetActive(true);
        pieDerecha.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        isSquished = false;


        squishedCollider.enabled = false;
        normalCollider.enabled = true;
        _rb.gravityScale = currentGravity;

    }

    private IEnumerator BecomeSquished()
    {
        // Estás en modo aplastado. Aquí puedes limitar movimiento si lo deseas.
        //yield return new WaitForSeconds(3f); // Ejemplo: se desaplasta en 3 segundos
        yield return null;

        //Prendo las extremidades
        brazoDerecha.SetActive(true);
        brazoIzquierda.SetActive(true);
        pieIzquierda.SetActive(true);
        pieDerecha.SetActive(true);

        animator.SetBool("IsSquished", false);
        squishedCollider.enabled = false;
        _rb.gravityScale = currentGravity;
        normalCollider.enabled = true;
        isSquished = false;
    }

    private void Respawn()
    {
        Debug.Log("RESPAWNING");
        var currentFlag = RespawnManager.instance.GetCurrentRespawnFlag();
        transform.position = new Vector3(
            currentFlag.respawnPosition.position.x - Random.Range(-currentFlag.respawnDistances, currentFlag.respawnDistances),
            currentFlag.respawnPosition.position.y,
            currentFlag.respawnPosition.position.z
        );
    }

    public void Jump()
    {
        animator.Play("StartJump");
        photonView.RPC("SincronizarSalto", RpcTarget.Others, true);  // Enviamos la animación también
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    [PunRPC]
    void SincronizarSalto(bool saltando)
    {
        // Sincronizar la animación de salto
        animator.Play("StartJump");
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
        //if (targetPlayer == photonView.Owner && changedProps.ContainsKey("color"))
        //{
        //    ApplyColorFromProperties(targetPlayer);
        //}
    }
    public void OnPlayerEnteredRoom(Player newPlayer) { }
    public void OnPlayerLeftRoom(Player otherPlayer) { }
    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }
    public void OnMasterClientSwitched(Player newMasterClient) { }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    }
}
