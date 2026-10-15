using UnityEngine;
using UnityEngine.InputSystem;

// Third-person orbit camera. The mouse turns it around the target, and a sphere cast
// pulls it in front of walls so it doesn't clip through them indoors.
// Look input comes from PlayerController; this script only applies it.
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Found by the Player tag if left empty.")]
    [SerializeField] Transform target;
    [Tooltip("Height of the orbit point above the target's feet.")]
    [SerializeField] float pivotHeight = 1.6f;

    [Header("Orbit")]
    [SerializeField] float distance = 4f;
    [Tooltip("Degrees per pixel of mouse movement.")]
    [SerializeField] float sensitivity = 0.1f;
    [SerializeField] float minPitch = -30f;
    [SerializeField] float maxPitch = 70f;

    [Header("Collision")]
    [Tooltip("Layers the camera can't pass through. Keep Player, Infected and Interactable off.")]
    [SerializeField] LayerMask collisionMask = 1; // Default
    [SerializeField] float collisionRadius = 0.25f;
    [Tooltip("How fast the camera moves back out once a wall stops blocking it (m/s).")]
    [SerializeField] float returnSpeed = 6f;

    float yaw;
    float pitch = 15f;
    float currentDistance;
    bool cursorLocked;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                Debug.LogWarning("CameraController: no target and no object tagged Player.", this);
        }

        if (target != null)
            yaw = target.eulerAngles.y;

        currentDistance = distance;
        SetCursorLocked(true);
    }

    public void AddLookInput(Vector2 delta)
    {
        // Mouse delta is already per frame, so it isn't scaled by deltaTime.
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        yaw += delta.x * sensitivity;
        pitch = Mathf.Clamp(pitch - delta.y * sensitivity, minPitch, maxPitch);
    }

    // UI panels call this with false to free the mouse, and true to take it back.
    public void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void Update()
    {
        // The editor frees the cursor on Escape; clicking the Game view grabs it again.
        if (cursorLocked && Cursor.lockState != CursorLockMode.Locked
            && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            SetCursorLocked(true);
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = target.position + Vector3.up * pivotHeight;
        Vector3 back = rotation * Vector3.back;

        float wantedDistance = distance;
        if (Physics.SphereCast(pivot, collisionRadius, back, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
            wantedDistance = hit.distance;

        // Snap in at once so walls never show through; ease back out so it doesn't pop.
        currentDistance = wantedDistance < currentDistance
            ? wantedDistance
            : Mathf.MoveTowards(currentDistance, wantedDistance, returnSpeed * Time.deltaTime);

        transform.SetPositionAndRotation(pivot + back * currentDistance, rotation);
    }
}
