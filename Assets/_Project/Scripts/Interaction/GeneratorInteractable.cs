using UnityEngine;

// The Warehouse's backup generator. Fuelling it switches on its running light, turns off
// the red no-power warnings and powers the vault door open. Used up after that.
public class GeneratorInteractable : RequiresItemInteractable
{
    [Tooltip("Turned on once the generator runs (lights, effects).")]
    [SerializeField] GameObject[] runningObjects;
    [Tooltip("Turned off once the generator runs (the red no-power warnings).")]
    [SerializeField] GameObject[] stoppedObjects;
    [SerializeField] DoorOpener vaultDoor;

    protected override void OnItemUsed(PlayerInteractor interactor)
    {
        SetAllActive(runningObjects, true);
        SetAllActive(stoppedObjects, false);

        if (vaultDoor != null)
            vaultDoor.Open();

        enabled = false;
    }

    static void SetAllActive(GameObject[] objects, bool active)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }
}
