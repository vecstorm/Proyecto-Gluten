using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DefeatScreenSimple : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image panelFondo;
    [SerializeField] private Image imagenHija;

    [Header("Color y Tiempos")]
    [SerializeField] private Color colorPanel = new Color(0.357f, 0.188f, 0.188f, 1f);
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float delayImagen = 0.5f;
    [SerializeField] private float tiempoEnPantalla = 2f;

    private CharacterStats playerStats;

    private void Start()
    {
        // Asignamos el color elegido al panel y los hacemos invisibles
        panelFondo.color = new Color(colorPanel.r, colorPanel.g, colorPanel.b, 0f);
        imagenHija.color = new Color(imagenHija.color.r, imagenHija.color.g, imagenHija.color.b, 0f);

        // Evita que bloqueen clics cuando están invisibles
        panelFondo.raycastTarget = false;
        imagenHija.raycastTarget = false;
    }

    private void OnEnable()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<CharacterStats>();
        if (playerStats != null) playerStats.OnDeath += IniciarSecuencia;
    }

    private void OnDisable()
    {
        if (playerStats != null) playerStats.OnDeath -= IniciarSecuencia;
    }

    private void IniciarSecuencia()
    {
        StopAllCoroutines();
        StartCoroutine(Secuencia());
    }

    private IEnumerator Secuencia()
    {
        // Aparece solo el panel de fondo
        yield return Fade(panelFondo, 0f, 1f);
        yield return new WaitForSeconds(delayImagen);

        // Aparece la imagen hija encima y espera
        yield return Fade(imagenHija, 0f, 1f);
        yield return new WaitForSeconds(tiempoEnPantalla);

        // Desaparece la imagen hija y luego el panel
        yield return Fade(imagenHija, 1f, 0f);
        yield return Fade(panelFondo, 1f, 0f);
    }

    // Esta función sirve para cualquier Image
    private IEnumerator Fade(Image img, float inicio, float fin)
    {
        Color c = img.color;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(inicio, fin, t / fadeTime);
            img.color = c;
            yield return null;
        }
        c.a = fin;
        img.color = c;
    }
}
