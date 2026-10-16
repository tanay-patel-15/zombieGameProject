using UnityEngine;

// The UV Transit Gate at the loading dock. It scans for the UV Power Core but doesn't
// take it: the core is installed in the next scene. Stays usable.
public class UVGateInteractable : RequiresItemInteractable
{
    protected override void OnItemUsed(PlayerInteractor interactor)
    {
        // The transition to the Radio Relay Tower starts here once SceneTransitionManager
        // exists (Oct 18).
    }
}
