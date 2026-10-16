using UnityEngine;

// Base for objects that only work once the player has a certain item: the fuel cage
// (bolt cutters), the generator (fuel) and the UV gate (the core). Interact() does the
// shared part (check for the item, use it up if set to, show the messages, tick the
// objective) and each subclass only says what happens after that, in OnItemUsed().
public abstract class RequiresItemInteractable : Interactable
{
    [SerializeField] ItemData requiredItem;
    [Tooltip("Remove the item from the inventory when it's used (fuel), or keep it (bolt cutters, the core).")]
    [SerializeField] bool consumeItem;
    [Tooltip("Shown when the player doesn't have the item.")]
    [TextArea]
    [SerializeField] string missingItemMessage;
    [Tooltip("Optional. Shown once the item has been used.")]
    [TextArea]
    [SerializeField] string usedMessage;

    public ItemData RequiredItem => requiredItem;

    // Sealed so every subclass gets the same check; they override OnItemUsed instead.
    public sealed override void Interact(PlayerInteractor interactor)
    {
        if (!enabled)
            return;

        if (interactor.Inventory == null)
        {
            Debug.LogWarning($"{GetType().Name}: no Inventory in the scene. Is the Systems prefab missing?", this);
            return;
        }

        if (!interactor.Inventory.HasItem(requiredItem))
        {
            interactor.ShowMessage(missingItemMessage);
            return;
        }

        if (consumeItem)
            interactor.Inventory.RemoveItem(requiredItem);

        interactor.ShowMessage(usedMessage);
        CompleteObjective(interactor);
        OnItemUsed(interactor);
    }

    // What the object does once the item has been used: open a door, start up, ...
    protected abstract void OnItemUsed(PlayerInteractor interactor);
}
