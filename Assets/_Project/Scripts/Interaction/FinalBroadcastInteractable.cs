using System;
using System.Collections;
using UnityEngine;

// The broadcast controls on the top platform. Locked until the antenna array is aligned;
// then it fires the citywide UV broadcast, holds the player still while the lights
// change, and announces the ending for EndingUI. Works once.
public class FinalBroadcastInteractable : Interactable
{
    // Fired after the broadcast effects have played. Static so the persistent HUD doesn't
    // need a reference to this scene.
    public static event Action BroadcastFired;

    [SerializeField] AntennaPuzzleController antennaPuzzle;
    [SerializeField] BroadcastEffects effects;
    [TextArea]
    [SerializeField] string lockedMessage = "Broadcast locked. The antenna array isn't aligned.";
    [Tooltip("Seconds of the light show before the ending panel appears.")]
    [SerializeField] float endingDelay = 3.5f;

    // Domain reload is off in this project, so statics survive between play sessions.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        BroadcastFired = null;
    }

    public override void Interact(PlayerInteractor interactor)
    {
        if (!enabled)
            return;

        if (antennaPuzzle == null || !antennaPuzzle.IsSolved)
        {
            interactor.ShowMessage(lockedMessage);
            return;
        }

        CompleteObjective(interactor);
        if (effects != null)
            effects.Play();

        // Started before disabling, so the ending still follows.
        StartCoroutine(AnnounceEnding());
        enabled = false;

        if (interactor.TryGetComponent(out PlayerController controller))
            controller.SetInputLocked(true);
    }

    IEnumerator AnnounceEnding()
    {
        yield return new WaitForSeconds(endingDelay);
        BroadcastFired?.Invoke();
    }
}
