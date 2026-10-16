using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Moves the player between scenes: locks input, fades to black, loads the scene, and fades
// back in once the matching PlayerSpawnPoint has placed the player. Lives on the persistent
// Systems object. Each time a scene starts it saves the inventory, so a retry can restart
// the scene with what the player arrived with.
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    // The ID of the spawn point to arrive at; PlayerSpawnPoint reads it as the scene wakes.
    // Null when the scene was opened directly, so the player stays where the scene put them.
    public static string PendingSpawnId { get; private set; }

    [SerializeField] FadeController fade;
    [SerializeField] float fadeDuration = 0.6f;

    public bool IsTransitioning { get; private set; }

    // Domain reload is off in this project, so statics survive between play sessions.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
        PendingSpawnId = null;
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // The first scene isn't loaded through here, so save its starting inventory now.
    void Start()
    {
        SaveCheckpoint();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // Requests while a transition is running are ignored, so a second press does nothing.
    public void LoadScene(string sceneName, string spawnId)
    {
        if (IsTransitioning)
            return;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"SceneTransitionManager: \"{sceneName}\" isn't in Build Profiles.", this);
            return;
        }

        StartCoroutine(Transition(sceneName, spawnId, false));
    }

    // Retry: restarts the current scene at the spawn point the player arrived at, with the
    // inventory they arrived with.
    public void ReloadScene()
    {
        if (IsTransitioning)
            return;

        StartCoroutine(Transition(SceneManager.GetActiveScene().name, PendingSpawnId, true));
    }

    IEnumerator Transition(string sceneName, string spawnId, bool restoreInventory)
    {
        IsTransitioning = true;
        SetPlayerInputLocked(true);
        yield return fade.FadeTo(1f, fadeDuration);

        // Done behind the black screen so the inventory bar doesn't change in view.
        if (restoreInventory && Inventory.Instance != null)
            Inventory.Instance.RestoreCheckpoint();

        PendingSpawnId = spawnId;
        Time.timeScale = 1f;
        yield return SceneManager.LoadSceneAsync(sceneName);

        // The new scene is awake and its player placed; keep them still until it's visible.
        SaveCheckpoint();
        SetPlayerInputLocked(true);
        yield return fade.FadeTo(0f, fadeDuration);
        SetPlayerInputLocked(false);
        IsTransitioning = false;
    }

    void SaveCheckpoint()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.SaveCheckpoint();
    }

    // The player belongs to the scene, so it's looked up each time.
    static void SetPlayerInputLocked(bool locked)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && player.TryGetComponent(out PlayerController controller))
            controller.SetInputLocked(locked);
    }
}
