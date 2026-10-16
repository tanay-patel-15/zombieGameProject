using UnityEngine;
using UnityEngine.InputSystem;

// Reads input from the project-wide actions (InputSystem_Actions) and hands it on:
// movement to PlayerMotor, mouse look to CameraController, E to PlayerInteractor.
// It doesn't move anything itself.
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("Found on the main camera if left empty.")]
    [SerializeField] CameraController cameraController;

    PlayerMotor motor;
    PlayerInteractor interactor;
    Transform view;
    InputAction moveAction;
    InputAction lookAction;
    InputAction sprintAction;
    InputAction jumpAction;
    InputAction interactAction;

    // While locked, input is ignored (scene transitions, end panels). Gravity still applies.
    public bool InputLocked { get; private set; }

    void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        interactor = GetComponent<PlayerInteractor>();

        InputActionAsset actions = InputSystem.actions;
        moveAction = actions.FindAction("Player/Move", true);
        lookAction = actions.FindAction("Player/Look", true);
        sprintAction = actions.FindAction("Player/Sprint", true);
        jumpAction = actions.FindAction("Player/Jump", true);
        interactAction = actions.FindAction("Player/Interact", true);
    }

    void Start()
    {
        if (cameraController == null && Camera.main != null)
            cameraController = Camera.main.GetComponent<CameraController>();

        if (cameraController != null)
            view = cameraController.transform;
        else if (Camera.main != null)
            view = Camera.main.transform;
        else
            Debug.LogWarning("PlayerController: no camera found, movement won't be camera-relative.", this);
    }

    void Update()
    {
        if (InputLocked)
        {
            motor.Move(Vector3.zero, false, false);
            return;
        }

        // While a reading panel is open the player stands still; E still works, to close it.
        if (interactor != null && interactor.IsReading)
        {
            motor.Move(Vector3.zero, false, false);
        }
        else
        {
            if (cameraController != null)
                cameraController.AddLookInput(lookAction.ReadValue<Vector2>());

            Vector3 direction = GetCameraRelativeDirection(moveAction.ReadValue<Vector2>());
            motor.Move(direction, sprintAction.IsPressed(), jumpAction.WasPressedThisFrame());
        }

        if (interactor != null && interactAction.WasPressedThisFrame())
            interactor.Interact();
    }

    // Also switches the interactor off while locked, which closes an open reading panel and
    // hides the prompt.
    public void SetInputLocked(bool locked)
    {
        InputLocked = locked;
        if (interactor != null)
            interactor.enabled = !locked;
    }

    // W moves toward where the camera faces, flattened onto the ground.
    Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (view == null)
            return new Vector3(input.x, 0f, input.y);

        Vector3 forward = Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(view.right, Vector3.up).normalized;
        return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
    }
}
