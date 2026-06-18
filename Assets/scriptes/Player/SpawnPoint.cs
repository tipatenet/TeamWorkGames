using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Visuel Gizmo (éditeur uniquement)")]
    public Color gizmoColor = Color.green;
    public float gizmoRadius = 0.5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1f);
        Gizmos.DrawSphere(transform.position + transform.forward * 1f, 0.1f);
    }
}