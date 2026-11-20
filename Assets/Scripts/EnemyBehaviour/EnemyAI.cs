using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof (CharacterStats))]
public abstract class EnemyAI : MonoBehaviour
{
    [Header("Debug:")]
    [SerializeField] protected bool showGizmos = true;
    [SerializeField] protected bool canRespawn = true;
    [SerializeField] protected bool isAerial = false;
    [SerializeField] protected float flashTimer = 0.2f;
    [SerializeField] protected GameObject stunnedIcon;
    [SerializeField] protected LayerMask playerDetectionLayer;
    [SerializeField] protected Transform targetTransform;

    protected Vector3 spawnPosition;
    protected Vector3 spawnRotation;
    protected bool invertedDirection = false;
    protected bool isDead = false;
    protected Color rendererColor;
    protected bool tryiedFlipAirborne = false;
    protected bool colorFlashing = false;

    protected Vector3 localMovement;
    protected Vector3 externalMovement;

    protected Rigidbody2D rb;
    protected CharacterStats characterStats;
    protected readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    protected virtual void InitializeBasicComponents()
    {
        EnemyManager.EnemiesOnMap.Add(this);

        rendererColor = GetComponent<SpriteRenderer>().color;
        invertedDirection = transform.rotation == Quaternion.Euler(0, 180, 0);
        rb = GetComponent<Rigidbody2D>();
        characterStats = GetComponent<CharacterStats>();
    }

    protected virtual void Flip()
    { 
        invertedDirection = !invertedDirection;

        transform.Rotate(0, 180, 0);
    }

    protected void FlashColor()
    {
        StartCoroutine(FlashSequence());
        //ParticleManager.SpawnParticles("AttackImpactParticles", transform);
    }

    protected void FlashColor(Color flashColor)
    {
        StartCoroutine(FlashSequence(flashColor));
    }

    private IEnumerator FlashSequence()
    {
        colorFlashing = true;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = Color.white;
        }

        yield return new WaitForSeconds(flashTimer);

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = rendererColor;
        }

        colorFlashing = false;
    }

    private IEnumerator FlashSequence(Color flashColor)
    {
        colorFlashing = true;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = flashColor;
        }

        yield return new WaitForSeconds(flashTimer);

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = rendererColor;
        }

        colorFlashing = false;
    }

    protected void ResetColor()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = rendererColor;
        }
    }

    protected void ChangeColor(Color newColor)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = newColor;
        }
    }

    // Devuelve si el jugador esta detras del enemigo o no
    protected bool TargetBehind(Vector3 targetPosition)
    {
        float deltaX = targetPosition.x - transform.position.x;

        return (deltaX < 0) != invertedDirection;
    }

    // Devuelve si el jugador esta detras del enemigo o no
    protected bool TargetBehind(Vector3 targetPosition, Vector3 positionToCompare)
    {
        float deltaX = targetPosition.x - positionToCompare.x;

        return (deltaX < 0) != invertedDirection;
    }

    public void SetTargetTransform(Transform newTargetTransform)
    {
        targetTransform = newTargetTransform;
    }

    public abstract void ResetEnemy();
}
