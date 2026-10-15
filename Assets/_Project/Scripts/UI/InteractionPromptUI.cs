using TMPro;
using UnityEngine;

// The "[E] Pick up Bolt Cutters" prompt at the bottom of the screen. Listens to
// PlayerInteractor and hides when nothing is in reach.
public class InteractionPromptUI : MonoBehaviour
{
    [Tooltip("The child panel to show and hide. Not this object, so the listener stays enabled.")]
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text label;
    [SerializeField] string keyLabel = "E";

    void Awake()
    {
        panel.SetActive(false);
    }

    void OnEnable()
    {
        PlayerInteractor.PromptChanged += Show;
    }

    void OnDisable()
    {
        PlayerInteractor.PromptChanged -= Show;
    }

    void Show(string prompt)
    {
        bool visible = !string.IsNullOrEmpty(prompt);
        panel.SetActive(visible);
        if (visible)
            label.text = $"[{keyLabel}] {prompt}";
    }
}
