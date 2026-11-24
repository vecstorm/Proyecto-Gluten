using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VictoryNabager : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image panelFondo;
    [SerializeField] private Image imagenHija;

    [Header("Color del panel")]
    [SerializeField] private Color colorPanel = new Color(0.357f, 0.188f, 0.188f, 1f);

    [Header("Tiempos del panel")]
    [SerializeField] private float inicioPanel = 0f;
    [SerializeField] private float desaparicionPanel = 2f;

    [Header("Tiempos de la imagen")]
    [SerializeField] private float inicioImagen = 0.5f;
    [SerializeField] private float desaparicionImagen = 2f;

    [Header("Duración del Fade")]
    [SerializeField] private float fadeTime = 1f;

    private CharacterStats playerStats;

    private void Start()
    {
        // Asignamos el color elegido y hacemos invisibles los elementos
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
        // Esperamos y aparece el panel
        yield return new WaitForSeconds(inicioPanel);
        yield return Fade(panelFondo, 0f, 1f);

        // Esperamos y aparece la imagen
        yield return new WaitForSeconds(inicioImagen);
        yield return Fade(imagenHija, 0f, 1f);

        // Esperamos y desaparece la imagen
        yield return new WaitForSeconds(desaparicionImagen);
        yield return Fade(imagenHija, 1f, 0f);

        // Esperamos y desaparece el panel
        yield return new WaitForSeconds(desaparicionPanel);
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