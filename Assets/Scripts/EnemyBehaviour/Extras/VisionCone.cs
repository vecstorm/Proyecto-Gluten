using UnityEngine;

public class VisionCone : MonoBehaviour
{
    [SerializeField] float coneRange = 10f;
    [SerializeField] float coneAngle = 45f;
    [SerializeField] int rayNumber = 20;
    [SerializeField] LayerMask layerToDetect;

    private bool disabled = false;
    private bool targetDetected;
    private Transform targetTransform;

    private void Update()
    {
        targetDetected = false;
        targetTransform = null;

        if (disabled) return;

        for (int i = 0; i < rayNumber; i++)
        {
            float raycastAngle = (-coneAngle * 0.5f) + (coneAngle * i) / (rayNumber - 1);

            Vector2 raycastDirection = Quaternion.AngleAxis(raycastAngle, Vector3.forward) * transform.right;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, raycastDirection, coneRange, layerToDetect);

            bool collided = hit.collider != null;

            if (collided && hit.transform.CompareTag("Player"))
            {
                targetDetected = true;
                targetTransform = hit.transform;
            }

            Debug.DrawRay(transform.position, raycastDirection * (collided ? hit.distance : coneRange), (collided ? (targetDetected ? Color.green : Color.yellow) : Color.red));
        }
    }

    public bool DetectedTarget()
    {
        return targetDetected;
    }

    public Transform GetTargetTransform()
    {
        return targetTransform;
    }

    public void DisableVision()
    {
        disabled = true;
    }

    public void EnableVision()
    {
        disabled = false;
    }
}
