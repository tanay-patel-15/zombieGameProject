using System.Collections;
using UnityEngine;

// The black screen scene transitions fade through. It has its own canvas above the HUD
// and lives on the persistent Systems object, so it stays up while the next scene loads.
[RequireComponent(typeof(CanvasGroup))]
public class FadeController : MonoBehaviour
{
    CanvasGroup group;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        SetAlpha(0f);
    }

    // Unscaled, so a fade still runs if the game is paused behind an end panel.
    public IEnumerator FadeTo(float alpha, float duration)
    {
        float start = group.alpha;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            SetAlpha(Mathf.Lerp(start, alpha, t / duration));
            yield return null;
        }
        SetAlpha(alpha);
    }

    void SetAlpha(float alpha)
    {
        group.alpha = alpha;
        // While any of it shows, clicks don't reach the panels underneath.
        group.blocksRaycasts = alpha > 0f;
    }
}
