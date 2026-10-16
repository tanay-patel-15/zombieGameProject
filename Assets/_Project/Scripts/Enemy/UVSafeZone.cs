using System.Collections.Generic;
using UnityEngine;

// A pool of UV light under one of the purple lamps. InfectedAI never sees or chases a
// player standing in one, and a NavMeshModifierVolume on the same object keeps their
// paths out of it. The first time the player steps into one, the HUD explains why.
public class UVSafeZone : MonoBehaviour
{
    static readonly List<UVSafeZone> zones = new List<UVSafeZone>();
    static bool hintShown;

    [Tooltip("Radius of the safe area on the floor (m).")]
    [SerializeField] float radius = 2.5f;
    [Tooltip("How far above the zone's origin it still counts (m).")]
    [SerializeField] float height = 3f;
    [TextArea]
    [SerializeField] string hint = "UV light. The infected won't come into it.";

    PlayerInteractor player;

    // Domain reload is off in this project, so statics survive between play sessions.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        zones.Clear();
        hintShown = false;
    }

    void OnEnable()
    {
        zones.Add(this);
    }

    void OnDisable()
    {
        zones.Remove(this);
    }

    public static bool IsSafe(Vector3 position)
    {
        foreach (UVSafeZone zone in zones)
        {
            if (zone.Contains(position))
                return true;
        }
        return false;
    }

    void Update()
    {
        if (hintShown)
            return;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject == null)
                return;
            player = playerObject.GetComponent<PlayerInteractor>();
        }

        if (player != null && Contains(player.transform.position))
        {
            hintShown = true;
            player.ShowMessage(hint);
        }
    }

    bool Contains(Vector3 position)
    {
        Vector3 offset = position - transform.position;
        if (offset.y < -0.5f || offset.y > height)
            return false;
        offset.y = 0f;
        return offset.sqrMagnitude <= radius * radius;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.55f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
