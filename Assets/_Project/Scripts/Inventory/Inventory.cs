using System;
using System.Collections.Generic;
using UnityEngine;

// Holds the items the player is carrying. Lives on the Systems prefab so it can
// persist across scenes. The list is private; other scripts go through the methods
// below and listen to Changed to update themselves.
public class Inventory : MonoBehaviour
{
    // The first Inventory to wake up wins, so a duplicate Systems object loaded with a
    // later scene never replaces the one carrying the player's items.
    public static Inventory Instance { get; private set; }

    [SerializeField] List<ItemData> items = new List<ItemData>();

    // Fired after every add or remove.
    public event Action Changed;

    public IReadOnlyList<ItemData> Items => items;

    // Domain reload is off in this project, so statics survive between play sessions.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // Quest items are unique, so an item already held isn't added again.
    public bool AddItem(ItemData item)
    {
        if (item == null || items.Contains(item))
            return false;

        items.Add(item);
        Changed?.Invoke();
        return true;
    }

    public bool HasItem(ItemData item)
    {
        return item != null && items.Contains(item);
    }

    public bool RemoveItem(ItemData item)
    {
        if (!items.Remove(item))
            return false;

        Changed?.Invoke();
        return true;
    }
}
