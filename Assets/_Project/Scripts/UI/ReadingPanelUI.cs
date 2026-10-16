using TMPro;
using UnityEngine;

// The large text panel a terminal opens (the Warehouse briefing). Listens to
// PlayerInteractor; while it's open the prompt below reads "[E] Close".
public class ReadingPanelUI : MonoBehaviour
{
    [Tooltip("The child panel to show and hide. Not this object, so the listener stays enabled.")]
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text body;

    void Awake()
    {
        panel.SetActive(false);
    }

    void OnEnable()
    {
        PlayerInteractor.ReadingChanged += Show;
    }

    void OnDisable()
    {
        PlayerInteractor.ReadingChanged -= Show;
    }

    // A null body means the panel closed.
    void Show(string titleText, string bodyText)
    {
        bool visible = bodyText != null;
        panel.SetActive(visible);
        if (!visible)
            return;

        title.text = titleText;
        title.gameObject.SetActive(!string.IsNullOrEmpty(titleText));
        body.text = bodyText;
    }
}
