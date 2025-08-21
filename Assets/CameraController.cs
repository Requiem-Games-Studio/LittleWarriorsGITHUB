using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public Camera cam;

    public float m_DampTime = 0.2f;           // Tiempo de suavizado al mover o hacer zoom.
    public float m_ScreenEdgeBuffer = 2f;     // Margen entre jugadores y borde de pantalla.
    public float m_MinSize = 4f;              // Zoom mínimo (más cerca).
    public float m_MaxSize = 20f;             // Zoom máximo (más lejos).

    public List<Transform> m_Targets;         // Jugadores a seguir.

    private Camera m_Camera;
    private float m_ZoomSpeed;
    private Vector3 m_MoveVelocity;
    private Vector3 m_DesiredPosition;

    public float worldMinX = float.MinValue;
    public float worldMaxX = float.MaxValue;


    private void Awake()
    {
        instance = this;
        m_Camera = Camera.main;
    }

    private void FixedUpdate()
    {
        Move();
        Zoom();
    }
    private void LateUpdate()
    {
        UpdateWorldLimits();
    }

    private void UpdateWorldLimits()
    {
        float camSize = m_Camera.orthographicSize;
        float horzExtent = camSize * m_Camera.aspect;
        Vector3 camPos = transform.position;

        worldMinX = camPos.x - horzExtent;
        worldMaxX = camPos.x + horzExtent;
    }

    public void Move()
    {
        FindBoundingBoxCenter();
        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    }

    private void FindBoundingBoxCenter()
    {
        if (m_Targets.Count == 0)
            return;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (Transform target in m_Targets)
        {
            if (!target.gameObject.activeSelf)
                continue;

            Vector3 pos = target.position;

            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);
        }

        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;

        m_DesiredPosition = new Vector3(centerX, centerY, transform.position.z);
    }

    private void Zoom()
    {
        float requiredSize = FindRequiredSize();
        m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
    }

    private float FindRequiredSize()
    {
        if (m_Targets.Count == 0)
            return m_MinSize;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (Transform target in m_Targets)
        {
            if (!target.gameObject.activeSelf)
                continue;

            Vector3 pos = target.position;

            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);
        }

        float width = maxX - minX;
        float height = maxY - minY;

        float requiredSize = Mathf.Max(height / 2f, (width / 2f) / m_Camera.aspect);
        requiredSize += m_ScreenEdgeBuffer;

        return Mathf.Clamp(requiredSize, m_MinSize, m_MaxSize);
    }

    public void SetStartPositionAndSize()
    {
        FindBoundingBoxCenter();
        transform.position = m_DesiredPosition;
        m_Camera.orthographicSize = FindRequiredSize();
    }
}