using TMPro;
using UnityEngine;

// Short feedback lines above the prompt ("The cage is chained shut."). Listens to
// PlayerInteractor and hides each message after a time that grows with its length; a
// new message replaces the one showing.
public class MessageUI : MonoBehaviour
{
    [Tooltip("The child panel to show and hide. Not this object, so the listener stays enabled.")]
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text label;
    [SerializeField] float minSeconds = 2.5f;
    [SerializeField] float secondsPerCharacter = 0.05f;

    float hideAt;

    void Awake()
    {
        panel.SetActive(false);
    }

    void OnEnable()
    {
        PlayerInteractor.MessageShown += Show;
    }

    void OnDisable()
    {
        PlayerInteractor.MessageShown -= Show;
    }

    // Unscaled, so a message still disappears if the game is paused behind an end panel.
    void Update()
    {
        if (panel.activeSelf && Time.unscaledTime >= hideAt)
            panel.SetActive(false);
    }

    void Show(string message)
    {
        label.text = message;
        panel.SetActive(true);
        hideAt = Time.unscaledTime + Mathf.Max(minSeconds, message.Length * secondsPerCharacter);
    }
}
