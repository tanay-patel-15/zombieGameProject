using UnityEngine;

// Where the player arrives in a scene. SceneTransitionManager stores the ID to arrive at
// before loading; the matching point moves the scene's player onto itself as the scene
// wakes, before the camera reads the player's facing in Start.
public class PlayerSpawnPoint : MonoBehaviour
{
    [Tooltip("Matches the spawn ID the gate loads this scene with, e.g. \"FromWarehouse\".")]
    [SerializeField] string id;

    void Awake()
    {
        string pending = SceneTransitionManager.PendingSpawnId;
        if (string.IsNullOrEmpty(pending) || pending != id)
            return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null || !player.TryGetComponent(out PlayerMotor motor))
        {
            Debug.LogWarning($"PlayerSpawnPoint {id}: no Player with a PlayerMotor in the scene.", this);
            return;
        }

        motor.Teleport(transform.position, transform.rotation);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.9f, 0.4f);
        Gizmos.DrawLine(transform.position + Vector3.up * 0.9f, transform.position + Vector3.up * 0.9f + transform.forward);
    }
}
