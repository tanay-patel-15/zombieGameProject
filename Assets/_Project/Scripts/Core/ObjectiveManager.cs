using System;
using System.Collections.Generic;
using UnityEngine;

// The loaded scene's objectives, in the order the player is meant to do them. There is
// one per scene rather than on the persistent Systems object: a retry reloads the scene
// and starts its chain over, and what does cross scenes (the core) is in the Inventory.
// Interactables complete objectives through PlayerInteractor; the HUD listens to Changed.
public class ObjectiveManager : MonoBehaviour
{
    // The manager of the loaded scene.
    public static ObjectiveManager Instance { get; private set; }

    // Fired when the scene starts and after every completed objective. Static so the HUD
    // doesn't need a reference to whichever scene is loaded.
    public static event Action<ObjectiveManager> Changed;

    [SerializeField] List<ObjectiveData> objectives = new List<ObjectiveData>();

    readonly HashSet<ObjectiveData> completed = new HashSet<ObjectiveData>();

    public IReadOnlyList<ObjectiveData> Objectives => objectives;

    // The first objective in the list that isn't done, or null once all of them are.
    public ObjectiveData Current
    {
        get
        {
            foreach (ObjectiveData objective in objectives)
            {
                if (objective != null && !completed.Contains(objective))
                    return objective;
            }
            return null;
        }
    }

    // Domain reload is off in this project, so statics survive between play sessions.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
        Changed = null;
    }

    // Last one wins: during a scene load the new scene can wake up before the old one is
    // destroyed.
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Changed?.Invoke(this);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool IsComplete(ObjectiveData objective)
    {
        return objective != null && completed.Contains(objective);
    }

    // Steps done out of order are ticked too; Current stays on the first one left.
    public void Complete(ObjectiveData objective)
    {
        if (objective == null)
            return;

        if (!objectives.Contains(objective))
        {
            Debug.LogWarning($"ObjectiveManager: {objective.name} isn't in this scene's list.", this);
            return;
        }

        if (completed.Add(objective))
            Changed?.Invoke(this);
    }
}
