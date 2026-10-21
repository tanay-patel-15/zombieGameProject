using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// What the player sees when the UV broadcast fires: the beam switches on and the city's
// lights fade to UV blue and purple. FinalBroadcastInteractable calls Play().
public class BroadcastEffects : MonoBehaviour
{
    [Tooltip("Hidden until the broadcast fires: the beam and its glow.")]
    [SerializeField] GameObject[] showOnPlay;
    [Tooltip("Every Light under these is recoloured.")]
    [SerializeField] Transform[] lightRoots;
    [Tooltip("The lights alternate between these.")]
    [SerializeField] Color[] uvColors = { new Color(0.35f, 0.3f, 1f), new Color(0.6f, 0.2f, 1f) };
    [SerializeField] float intensityMultiplier = 1.5f;
    [SerializeField] Color ambientColor = new Color(0.12f, 0.08f, 0.25f);
    [SerializeField] float fadeDuration = 2f;

    void Awake()
    {
        foreach (GameObject shown in showOnPlay)
        {
            if (shown != null)
                shown.SetActive(false);
        }
    }

    public void Play()
    {
        foreach (GameObject shown in showOnPlay)
        {
            if (shown != null)
                shown.SetActive(true);
        }

        StartCoroutine(FadeLights());
    }

    IEnumerator FadeLights()
    {
        var lights = new List<Light>();
        foreach (Transform root in lightRoots)
        {
            if (root != null)
                lights.AddRange(root.GetComponentsInChildren<Light>());
        }

        var startColors = new Color[lights.Count];
        var startIntensities = new float[lights.Count];
        for (int i = 0; i < lights.Count; i++)
        {
            startColors[i] = lights[i].color;
            startIntensities[i] = lights[i].intensity;
        }
        Color startAmbient = RenderSettings.ambientLight;

        for (float t = 0f; t < 1f; t += Time.deltaTime / fadeDuration)
        {
            Apply(lights, startColors, startIntensities, startAmbient, t);
            yield return null;
        }
        Apply(lights, startColors, startIntensities, startAmbient, 1f);
    }

    void Apply(List<Light> lights, Color[] startColors, float[] startIntensities, Color startAmbient, float t)
    {
        for (int i = 0; i < lights.Count; i++)
        {
            Color target = uvColors[i % uvColors.Length];
            lights[i].color = Color.Lerp(startColors[i], target, t);
            lights[i].intensity = Mathf.Lerp(startIntensities[i], startIntensities[i] * intensityMultiplier, t);
        }
        RenderSettings.ambientLight = Color.Lerp(startAmbient, ambientColor, t);
    }
}
