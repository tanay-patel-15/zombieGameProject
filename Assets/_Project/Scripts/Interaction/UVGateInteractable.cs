using UnityEngine;

// The UV Transit Gate. It scans for the UV Power Core but doesn't take it, since the core
// is installed in the next scene, and then sends the player to the Radio Relay Tower.
public class UVGateInteractable : RequiresItemInteractable
{
    [Tooltip("Loaded once the core is scanned. Must be in Build Profiles.")]
    [SerializeField] string destinationScene = "RadioRelayTower";
    [Tooltip("ID of the PlayerSpawnPoint the player arrives at.")]
    [SerializeField] string destinationSpawnId = "FromWarehouse";

    protected override void OnItemUsed(PlayerInteractor interactor)
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning("UVGateInteractable: no SceneTransitionManager. Is the Systems prefab missing?", this);
            return;
        }

        SceneTransitionManager.Instance.LoadScene(destinationScene, destinationSpawnId);
    }
}
