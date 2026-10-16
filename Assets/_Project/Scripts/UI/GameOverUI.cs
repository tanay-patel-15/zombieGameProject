using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The "caught" panel. Shown when an infected catches the player; Retry (or R) restarts the
// scene through SceneTransitionManager. It's on the persistent HUD, so it hides itself
// whenever a scene loads.
public class GameOverUI : MonoBehaviour
{
    [Tooltip("The child panel to show and hide. Not this object, so the listener stays enabled.")]
    [SerializeField] GameObject panel;
    [SerializeField] Button retryButton;

    void Awake()
    {
        panel.SetActive(false);
        retryButton.onClick.AddListener(Retry);
    }

    void OnEnable()
    {
        InfectedAI.PlayerCaught += Show;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        InfectedAI.PlayerCaught -= Show;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (panel.activeSelf && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            Retry();
    }

    void Show()
    {
        panel.SetActive(true);
        // Frees the mouse for the button; the next scene's camera locks it again.
        if (Camera.main != null && Camera.main.TryGetComponent(out CameraController cameraController))
            cameraController.SetCursorLocked(false);
    }

    void Retry()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.ReloadScene();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        panel.SetActive(false);
    }
}
