using UnityEngine;

// One of the three antenna panels on the tower. The panel doesn't know the right order:
// using it reports to the AntennaPuzzleController, which lights or resets the borders.
// The colour of its painted border is what links it to the sign beside the console.
public class AntennaPanelInteractable : Interactable
{
    [SerializeField] AntennaPuzzleController puzzle;
    [Tooltip("The painted frame around the panel.")]
    [SerializeField] Renderer[] borderRenderers;
    [Tooltip("The frame's paint while the panel isn't calibrated.")]
    [SerializeField] Material dimMaterial;
    [Tooltip("The frame once calibrated: the same colour, glowing.")]
    [SerializeField] Material litMaterial;

    public bool IsCalibrated { get; private set; }

    void Awake()
    {
        SetCalibrated(false);
    }

    public override void Interact(PlayerInteractor interactor)
    {
        if (!enabled)
            return;

        if (puzzle == null)
        {
            Debug.LogWarning("AntennaPanelInteractable: no AntennaPuzzleController assigned.", this);
            return;
        }

        puzzle.Calibrate(this, interactor);
    }

    // Called by the puzzle when the panel locks in or the sequence resets.
    public void SetCalibrated(bool calibrated)
    {
        IsCalibrated = calibrated;
        Material material = calibrated ? litMaterial : dimMaterial;
        if (material == null)
            return;

        foreach (Renderer border in borderRenderers)
        {
            if (border != null)
                border.sharedMaterial = material;
        }
    }
}
