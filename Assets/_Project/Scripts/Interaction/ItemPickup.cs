using UnityEngine;

// An item lying in the level. Using it puts the item in the inventory and hides the
// pickup. It only ever gives its item once: after that the component is disabled.
public class ItemPickup : Interactable
{
    [SerializeField] ItemData item;

    public ItemData Item => item;
    public override string Prompt => item != null ? $"Pick up {item.DisplayName}" : base.Prompt;

    public override void Interact(PlayerInteractor interactor)
    {
        if (!enabled)
            return;

        if (interactor.Inventory == null)
        {
            Debug.LogWarning("ItemPickup: no Inventory in the scene. Is the Systems prefab missing?", this);
            return;
        }

        if (interactor.Inventory.AddItem(item))
        {
            enabled = false;
            gameObject.SetActive(false);
        }
    }
}
