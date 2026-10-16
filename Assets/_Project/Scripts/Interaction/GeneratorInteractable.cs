using UnityEngine;

// The Warehouse's backup generator. Fuelling it switches on its running light and powers
// the vault door open. Used up after that.
public class GeneratorInteractable : RequiresItemInteractable
{
    [Tooltip("Turned on once the generator runs (lights, effects).")]
    [SerializeField] GameObject[] runningObjects;
    [SerializeField] DoorOpener vaultDoor;

    protected override void OnItemUsed(PlayerInteractor interactor)
    {
        foreach (GameObject runningObject in runningObjects)
        {
            if (runningObject != null)
                runningObject.SetActive(true);
        }

        if (vaultDoor != null)
            vaultDoor.Open();

        enabled = false;
    }
}
