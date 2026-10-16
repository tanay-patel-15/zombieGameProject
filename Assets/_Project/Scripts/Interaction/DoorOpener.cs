using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Slides a door open: the fuel cage door and the vault door. Whatever unlocks it (a
// LockedObjectInteractable, the generator) calls Open(); after the first call it does
// nothing. A NavMeshObstacle on the door is switched off once it's open.
public class DoorOpener : MonoBehaviour
{
    [Tooltip("How far the door moves when it opens, along its own axes (m).")]
    [SerializeField] Vector3 openOffset = new Vector3(0f, -2.5f, 0f);
    [SerializeField] float duration = 1.5f;

    public bool IsOpen { get; private set; }

    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        StartCoroutine(Slide());
    }

    IEnumerator Slide()
    {
        Vector3 start = transform.localPosition;
        Vector3 end = start + transform.localRotation * openOffset;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            transform.localPosition = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        transform.localPosition = end;

        // The doorway is clear now, so stop cutting it out of the infected's NavMesh.
        if (TryGetComponent(out NavMeshObstacle obstacle))
            obstacle.enabled = false;
    }
}
