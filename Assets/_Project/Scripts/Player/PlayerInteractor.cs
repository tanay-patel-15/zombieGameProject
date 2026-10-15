using System;
using UnityEngine;

// Finds the nearest Interactable the player can reach and see, and tells the HUD what
// to prompt. PlayerController calls Interact() when E is pressed; this script doesn't
// read input itself.
public class PlayerInteractor : MonoBehaviour
{
    // The focused interactable's prompt, or null when nothing is in reach. Static so the
    // HUD doesn't need a reference to whichever player is in the current scene.
    public static event Action<string> PromptChanged;

    [Tooltip("How far from the player's chest an interactable can be (m).")]
    [SerializeField] float radius = 1.8f;
    [Tooltip("Height above the player's feet that reach and line of sight are measured from.")]
    [SerializeField] float originHeight = 1.1f;
    [SerializeField] LayerMask interactableMask = 1 << 9; // Interactable
    [Tooltip("Layers that block sight of an interactable: walls, shelves, closed doors.")]
    [SerializeField] LayerMask obstructionMask = 1; // Default

    readonly Collider[] hits = new Collider[16];
    string currentPrompt;

    public Interactable Focus { get; private set; }
    public Inventory Inventory => Inventory.Instance;

    // Domain reload is off in this project, so statics survive between play sessions.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        PromptChanged = null;
    }

    void Update()
    {
        RefreshFocus();
    }

    void OnDisable()
    {
        Focus = null;
        SetPrompt(null);
    }

    public void Interact()
    {
        if (Focus == null || !Focus.isActiveAndEnabled)
            return;

        Focus.Interact(this);
        // The object may have been used up; update the prompt this frame.
        RefreshFocus();
    }

    void RefreshFocus()
    {
        Focus = FindNearest();
        SetPrompt(Focus != null ? Focus.Prompt : null);
    }

    Interactable FindNearest()
    {
        Vector3 origin = transform.position + Vector3.up * originHeight;
        int count = Physics.OverlapSphereNonAlloc(origin, radius, hits, interactableMask, QueryTriggerInteraction.Collide);

        Interactable nearest = null;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            Interactable candidate = hits[i].GetComponentInParent<Interactable>();
            if (candidate == null || !candidate.isActiveAndEnabled)
                continue;

            float distance = Vector3.Distance(origin, hits[i].ClosestPoint(origin));
            if (distance >= nearestDistance || !InSight(origin, hits[i], candidate))
                continue;

            nearest = candidate;
            nearestDistance = distance;
        }
        return nearest;
    }

    // Aims at the collider's center rather than its closest point, so a pickup resting
    // on a shelf isn't blocked by the shelf it sits on. The interactable's own solid
    // parts (a generator body) don't count as blocking.
    bool InSight(Vector3 origin, Collider target, Interactable candidate)
    {
        if (!Physics.Linecast(origin, target.bounds.center, out RaycastHit hit, obstructionMask, QueryTriggerInteraction.Ignore))
            return true;

        return hit.collider.GetComponentInParent<Interactable>() == candidate;
    }

    void SetPrompt(string prompt)
    {
        if (prompt == currentPrompt)
            return;

        currentPrompt = prompt;
        PromptChanged?.Invoke(prompt);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * originHeight, radius);
    }
}
