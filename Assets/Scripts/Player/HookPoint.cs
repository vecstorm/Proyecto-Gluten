using UnityEngine;

/// <summary>
/// Punto de enganche válido para el sistema de gancho.
/// Gestiona el feedback visual cuando el jugador se encuentra
/// dentro del rango de detección.
/// </summary>
public class HookPoint : MonoBehaviour
{
    #region Variables

    #region Visual

    [Header("Visual")]

    [Tooltip("Sprite que representa visualmente el punto de enganche")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Color por defecto del punto de enganche")]
    [SerializeField] private Color normalColor = Color.white;

    [Tooltip("Color mostrado cuando el punto está dentro del rango de enganche")]
    [SerializeField] private Color inRangeColor = Color.green;

    #endregion

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        SetInRange(false);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Actualiza el estado visual del punto de enganche.
    /// </summary>
    /// <param name="inRange">
    /// True si el jugador puede engancharse actualmente a este punto
    /// </param>
    public void SetInRange(bool inRange)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = inRange ? inRangeColor : normalColor;
    }

    #endregion
}