using UnityEngine;

// Something held shut until the player has the right item, like the chained fuel cage
// and the bolt cutters. Opens its door once and is then used up.
public class LockedObjectInteractable : RequiresItemInteractable
{
    [SerializeField] DoorOpener door;

    protected override void OnItemUsed(PlayerInteractor interactor)
    {
        if (door != null)
            door.Open();

        enabled = false;
    }
}
