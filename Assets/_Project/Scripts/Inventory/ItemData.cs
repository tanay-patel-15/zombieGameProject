using UnityEngine;

// One quest item (Bolt Cutters, Generator Fuel, ...). Each item is an asset in
// _Project/Data/Items; the inventory and interactables compare these assets directly.
[CreateAssetMenu(menuName = "Last Light/Item", fileName = "Item_")]
public class ItemData : ScriptableObject
{
    [Tooltip("Short unique key, e.g. bolt_cutters.")]
    [SerializeField] string id;
    [SerializeField] string displayName;
    [Tooltip("Optional. The inventory slot shows only the name without one.")]
    [SerializeField] Sprite icon;
    [TextArea]
    [SerializeField] string description;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public string Description => description;
}
