using UnityEngine;

// The persistent bootstrap on the Systems prefab. Every scene has its own Systems so it
// can be played on its own; the first one to wake up is kept across scene loads, and the
// copy that comes with the next scene removes itself before its HUD and Inventory wake.
// No gameplay logic here.
[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Domain reload is off in this project, so statics survive between play sessions.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Deactivating first means the copy's other components and children never wake.
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
