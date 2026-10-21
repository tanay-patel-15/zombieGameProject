using System.Collections;
using UnityEngine;

// A failing light: it stays on, and every few seconds it stutters, dropping out a few
// times in quick succession. Used on some of the Warehouse's ceiling lights.
[RequireComponent(typeof(Light))]
public class FailingLight : MonoBehaviour
{
    [Tooltip("Seconds between stutters (random in this range).")]
    [SerializeField] Vector2 interval = new Vector2(1.5f, 6f);
    [Tooltip("Drop-outs per stutter (random in this range).")]
    [SerializeField] Vector2Int dropouts = new Vector2Int(1, 4);
    [Tooltip("Brightness during a drop-out, as a fraction of the normal intensity.")]
    [SerializeField] float dimFraction = 0.1f;

    Light failingLight;
    float baseIntensity;

    void Awake()
    {
        failingLight = GetComponent<Light>();
        baseIntensity = failingLight.intensity;
    }

    void OnEnable()
    {
        StartCoroutine(Stutter());
    }

    void OnDisable()
    {
        failingLight.intensity = baseIntensity;
    }

    IEnumerator Stutter()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(interval.x, interval.y));

            int count = Random.Range(dropouts.x, dropouts.y + 1);
            for (int i = 0; i < count; i++)
            {
                failingLight.intensity = baseIntensity * dimFraction;
                yield return new WaitForSeconds(Random.Range(0.03f, 0.12f));
                failingLight.intensity = baseIntensity;
                yield return new WaitForSeconds(Random.Range(0.04f, 0.2f));
            }
        }
    }
}
