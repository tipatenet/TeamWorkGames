using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

        if (spawnPoint == null)
        {
            Debug.LogWarning("Aucun SpawnPoint trouvé dans la scène !");
            return;
        }

        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return; 

        Vector3 spawnPosition = spawnPoint.transform.position;

        SceneTransitionManager.Instance.FadeAndTeleport(() =>
        {
            pc.TeleportTo(spawnPosition);
        });
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
#endif
}