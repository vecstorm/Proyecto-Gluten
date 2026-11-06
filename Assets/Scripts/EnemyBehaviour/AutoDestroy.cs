using System.Collections;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1f;
    [SerializeField] private bool disableAutoDestruction = false;

    private float timer = 0;

    private void Update()
    {
        if (disableAutoDestruction) return;

        timer += Time.deltaTime;

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    public void SetLifeTime(float lifeTime)
    {
        this.lifeTime = lifeTime;
    }
}
