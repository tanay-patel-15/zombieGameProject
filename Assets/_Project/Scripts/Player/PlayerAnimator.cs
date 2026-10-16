using UnityEngine;

// Drives the player's Animator from PlayerMotor and PlayerInteractor: Speed for the
// idle/walk/run blend, Grounded and Jump for jumps and falls, and Interact for the button
// press on the upper-body layer. The Animator is on the Model child.
[RequireComponent(typeof(PlayerMotor))]
public class PlayerAnimator : MonoBehaviour
{
    [Tooltip("Seconds off the ground before the fall pose plays, so steps and stairs don't flicker it.")]
    [SerializeField] float airborneDelay = 0.15f;
    [SerializeField] float speedDampTime = 0.1f;

    static readonly int SpeedParam = Animator.StringToHash("Speed");
    static readonly int GroundedParam = Animator.StringToHash("Grounded");
    static readonly int JumpParam = Animator.StringToHash("Jump");
    static readonly int InteractParam = Animator.StringToHash("Interact");

    PlayerMotor motor;
    PlayerInteractor interactor;
    Animator animator;
    float airborneTime;

    void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        interactor = GetComponent<PlayerInteractor>();
        animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        motor.Jumped += OnJumped;
        if (interactor != null)
            interactor.Interacted += OnInteracted;
    }

    void OnDisable()
    {
        motor.Jumped -= OnJumped;
        if (interactor != null)
            interactor.Interacted -= OnInteracted;
    }

    void Update()
    {
        if (animator == null)
            return;

        airborneTime = motor.IsGrounded ? 0f : airborneTime + Time.deltaTime;
        animator.SetFloat(SpeedParam, motor.Speed, speedDampTime, Time.deltaTime);
        animator.SetBool(GroundedParam, airborneTime < airborneDelay);
    }

    // A jump counts as airborne straight away rather than after the delay.
    void OnJumped()
    {
        airborneTime = airborneDelay;
        if (animator == null)
            return;
        animator.SetBool(GroundedParam, false);
        animator.SetTrigger(JumpParam);
    }

    void OnInteracted()
    {
        if (animator != null)
            animator.SetTrigger(InteractParam);
    }
}
