using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class MapWheelZoom : MonoBehaviour, IScrollHandler
{
    [Header("Zoom con rueda")]
    [SerializeField] private float minZoom = 0.5f;   // alejar: se ve el mapa completo
    [SerializeField] private float maxZoom = 2.5f;   // acercar
    [SerializeField] private float zoomStep = 0.1f;  // salto por tick
    [SerializeField] private float startZoom = 3f;   // zoom al abrir el mapa

    private ScrollRect scrollRect;
    private RectTransform content;
    private Vector2 baseSize;
    private RectTransform[] zones;
    private Vector2[] basePositions;
    private Vector3[] baseScales;
    private float zoom = 1f;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;

        if (content == null)
        {
            Debug.LogWarning("MapWheelZone asigna MapaContenido en el campo Content del ScrollRect", this);
            enabled = false;
            return;
        }

        baseSize = content.sizeDelta;

        zones = new RectTransform[content.childCount];
        basePositions = new Vector2[zones.Length];
        baseScales = new Vector3[zones.Length];

        for (int i = 0; i < zones.Length; i++)
        {
            zones[i] = content.GetChild(i) as RectTransform;
            basePositions[i] = zones[i].anchoredPosition;
            baseScales[i] = zones[i].localScale;
        }
    }

    private void OnEnable()
    {
        if (content == null)
            return;

        zoom = Mathf.Clamp(startZoom, minZoom, maxZoom);
        content.anchoredPosition = Vector2.zero;
        ApplyZoom(1f);
        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
    }

    public void OnScroll(PointerEventData eventData)
    {
        float y = eventData.scrollDelta.y;

        if (Mathf.Approximately(y, 0f))
            return;

        float previousZoom = zoom;
        float direction = y > 0f ? 1f : -1f;
        zoom = Mathf.Clamp(zoom + direction * zoomStep, minZoom, maxZoom);

        ApplyZoom(previousZoom);
    }

    private void ApplyZoom(float previousZoom)
    {
        float factor = zoom / previousZoom;

        content.sizeDelta = baseSize * zoom;
        content.anchoredPosition *= factor;

        for (int i = 0; i < zones.Length; i++)
        {
            zones[i].anchoredPosition = basePositions[i] * zoom;
            zones[i].localScale = baseScales[i] * zoom;
        }

    }
}
