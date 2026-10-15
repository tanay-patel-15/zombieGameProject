using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One slot in the inventory bar: the item's icon (if it has one) and its name.
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] Image background;
    [SerializeField] Image icon;
    [SerializeField] TMP_Text label;
    [SerializeField] Color filledColor = new Color(0.08f, 0.09f, 0.12f, 0.85f);
    [SerializeField] Color emptyColor = new Color(0.08f, 0.09f, 0.12f, 0.35f);

    public void SetItem(ItemData item)
    {
        if (item == null)
        {
            Clear();
            return;
        }

        icon.sprite = item.Icon;
        icon.enabled = item.Icon != null;
        label.text = item.DisplayName;
        background.color = filledColor;
    }

    public void Clear()
    {
        icon.sprite = null;
        icon.enabled = false;
        label.text = string.Empty;
        background.color = emptyColor;
    }
}
