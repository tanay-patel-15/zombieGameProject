using UnityEngine;

// Moves the player with a CharacterController: walk, sprint, jump and gravity.
// PlayerController calls Move() once per frame with a world-space direction;
// the motor turns the player to face that direction.
[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed = 3.5f;
    [SerializeField] float sprintSpeed = 6.5f;
    [Tooltip("How quickly speed changes when starting, stopping or switching to sprint (m/s²).")]
    [SerializeField] float acceleration = 20f;
    [Tooltip("Degrees per second the player turns to face the move direction.")]
    [SerializeField] float turnSpeed = 720f;

    [Header("Jump")]
    [SerializeField] float jumpHeight = 1.2f;
    [SerializeField] float gravity = -20f;
    [Tooltip("Seconds after leaving the ground when a jump still counts. Helps on stairs and slopes.")]
    [SerializeField] float coyoteTime = 0.15f;

    CharacterController controller;
    Vector3 horizontalVelocity;
    float verticalVelocity;
    float lastGroundedTime = float.NegativeInfinity;

    // Read by the Animator script later.
    public float Speed => horizontalVelocity.magnitude;
    public bool IsGrounded => controller.isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // Moves the player instantly, e.g. to a spawn point. Safe to call before Awake.
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        // The controller keeps its own copy of the position, so it's off while the transform moves.
        controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        controller.enabled = true;
        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;
    }

    // direction is world space on the ground plane, length 0 to 1.
    public void Move(Vector3 direction, bool sprint, bool jump)
    {
        float dt = Time.deltaTime;

        float targetSpeed = sprint ? sprintSpeed : walkSpeed;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, direction * targetSpeed, acceleration * dt);

        if (controller.isGrounded)
        {
            lastGroundedTime = Time.time;
            // A small downward push keeps the controller grounded on slopes and steps.
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }

        if (jump && Time.time - lastGroundedTime <= coyoteTime)
        {
            verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);
            lastGroundedTime = float.NegativeInfinity; // one jump per landing
        }

        verticalVelocity += gravity * dt;
        CollisionFlags flags = controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * dt);

        // Hitting a ceiling ends the rise instead of sticking to it.
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            verticalVelocity = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion facing = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, turnSpeed * dt);
        }
    }
}
