using UnityEngine;

// Shows the inventory in the HUD. Redraws the fixed row of slots whenever the
// inventory changes: slot i shows item i, the rest are cleared.
public class InventoryUI : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    [Tooltip("Parent of the InventorySlotUI children, in display order.")]
    [SerializeField] Transform slotParent;

    InventorySlotUI[] slots;

    void Awake()
    {
        slots = slotParent.GetComponentsInChildren<InventorySlotUI>(true);
    }

    void OnEnable()
    {
        inventory.Changed += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        inventory.Changed -= Refresh;
    }

    void Refresh()
    {
        var items = inventory.Items;
        if (items.Count > slots.Length)
            Debug.LogWarning($"InventoryUI: {items.Count} items but only {slots.Length} slots.", this);

        for (int i = 0; i < slots.Length; i++)
            slots[i].SetItem(i < items.Count ? items[i] : null);
    }
}
