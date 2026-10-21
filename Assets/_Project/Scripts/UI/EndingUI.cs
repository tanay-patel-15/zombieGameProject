using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The ending panel ("UV Network Restored"). Shown when the final broadcast fires; Restart
// goes back to the first scene with an empty inventory and Quit closes the game. It's on
// the persistent HUD, so it hides itself whenever a scene loads.
public class EndingUI : MonoBehaviour
{
    [Tooltip("The child panel to show and hide. Not this object, so the listener stays enabled.")]
    [SerializeField] GameObject panel;
    [SerializeField] Button restartButton;
    [SerializeField] Button quitButton;
    [Tooltip("Loaded by Restart. Must be in Build Profiles.")]
    [SerializeField] string firstScene = "Warehouse";

    void Awake()
    {
        panel.SetActive(false);
        restartButton.onClick.AddListener(Restart);
        quitButton.onClick.AddListener(Quit);
    }

    void OnEnable()
    {
        FinalBroadcastInteractable.BroadcastFired += Show;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        FinalBroadcastInteractable.BroadcastFired -= Show;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Show()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
        // Frees the mouse for the buttons; the next scene's camera locks it again.
        if (Camera.main != null && Camera.main.TryGetComponent(out CameraController cameraController))
            cameraController.SetCursorLocked(false);
    }

    void Restart()
    {
        if (SceneTransitionManager.Instance == null || SceneTransitionManager.Instance.IsTransitioning)
            return;

        if (Inventory.Instance != null)
            Inventory.Instance.Clear();
        SceneTransitionManager.Instance.LoadScene(firstScene, null);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        panel.SetActive(false);
    }
}
