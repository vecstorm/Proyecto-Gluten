using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Transform contenedorEnemigos;
    private List<EnemyAI> sceneEnemies = new List<EnemyAI>();

    // Diccionario para guardar el color original de cada SpriteRenderer al arrancar la escena
    private Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 1. Buscamos automáticamente el Empty contenedor por su nombre en la escena
        GameObject containerObj = GameObject.Find("Enemies");

        if (containerObj != null)
        {
            contenedorEnemigos = containerObj.transform;
        }
        else
        {
            Debug.LogError("¡No se encontró ningún objeto llamado 'Enemies' en la escena!");
            return;
        }

        // 2. Buscamos todos los scripts que hereden de EnemyAI dentro del contenedor (incluyendo los inactivos)
        EnemyAI[] foundEnemies = contenedorEnemigos.GetComponentsInChildren<EnemyAI>(true);
        sceneEnemies.AddRange(foundEnemies);

        // 3. REGISTRAMOS EL COLOR ORIGINAL DE CADA SPRITE AL INICIAR
        foreach (var enemy in sceneEnemies)
        {
            if (enemy != null)
            {
                SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var rend in renderers)
                {
                    if (!originalColors.ContainsKey(rend))
                    {
                        // Guardamos el color con el que el diseñador pintó al enemigo en el Inspector (ej. el amarillo)
                        originalColors.Add(rend, rend.color);
                    }
                }
            }
        }

        Debug.Log($"GameManager: Se han registrado {sceneEnemies.Count} enemigos y sus colores originales.");
    }

    public void RespawnAllEnemies()
    {
        foreach (var enemy in sceneEnemies)
        {
            if (enemy != null)
            {
                // 1. Encendemos al enemigo
                enemy.gameObject.SetActive(true);

                // 2. Restauramos su color original
                SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (SpriteRenderer rend in renderers)
                {
                    if (originalColors.ContainsKey(rend))
                    {
                        rend.color = originalColors[rend];
                    }
                }

                // 3. Invocamos su ResetEnemy() mediante Reflection
                MethodInfo resetMethod = enemy.GetType().GetMethod(
                    "ResetEnemy",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
                );

                if (resetMethod != null)
                {
                    resetMethod.Invoke(enemy, null);
                }

                // ==========================================================
                // 4. FORZAR EL RESETEO DE LA DIRECCIÓN (invertedDirection)
                // ==========================================================
                // Como 'invertedDirection' es una variable protegida, la modificamos por Reflection:
                FieldInfo directionField = enemy.GetType().BaseType.GetField(
                    "invertedDirection",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (directionField != null)
                {
                    // La devolvemos a su estado inicial (falso, mirando hacia su lado original)
                    directionField.SetValue(enemy, false);
                }

                // Aseguramos también que el transform.rotation del eje Y vuelva a 0 (por si usaba Flip con rotación)
                enemy.transform.rotation = Quaternion.Euler(0, 0, 0);

                // Y limpiamos cualquier flip del sprite por si acaso
                foreach (var rend in renderers)
                {
                    rend.flipX = false;
                }
            }
        }

        Debug.Log("GameManager: Enemigos reseteados, colores restaurados y dirección rehecha.");
    }
}