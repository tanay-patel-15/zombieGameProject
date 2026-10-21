using UnityEngine;

// The antenna array's calibration. The three panels must be used in the order shown on
// the colour-coded sign beside the relay console (1 red, 2 yellow, 3 blue); a wrong panel
// darkens them all and the sequence starts over. The panels have no power until the
// console has its amplifier.
public class AntennaPuzzleController : MonoBehaviour
{
    [Tooltip("The panels in the correct order: red, yellow, blue.")]
    [SerializeField] AntennaPanelInteractable[] correctOrder;
    [Tooltip("Ticked once the array is aligned.")]
    [SerializeField] ObjectiveData objective;
    [SerializeField] string noPowerMessage = "No power. The relay console isn't online yet.";
    [Tooltip("{0} is the number done, {1} the total.")]
    [SerializeField] string calibratedMessage = "Panel calibrated ({0}/{1}).";
    [SerializeField] string alreadyCalibratedMessage = "This panel is already calibrated.";
    [SerializeField] string failedMessage = "Calibration failed. Sequence reset.";
    [TextArea]
    [SerializeField] string solvedMessage = "Antenna array aligned. The broadcast controls on the top platform are live.";

    int progress;

    public bool IsPowered { get; private set; }
    public bool IsSolved { get; private set; }

    // The relay console calls this once the amplifier is in.
    public void PowerOn()
    {
        IsPowered = true;
    }

    public void Calibrate(AntennaPanelInteractable panel, PlayerInteractor interactor)
    {
        if (!IsPowered)
        {
            interactor.ShowMessage(noPowerMessage);
            return;
        }

        if (IsSolved || panel.IsCalibrated)
        {
            interactor.ShowMessage(alreadyCalibratedMessage);
            return;
        }

        if (panel != correctOrder[progress])
        {
            foreach (AntennaPanelInteractable other in correctOrder)
                other.SetCalibrated(false);
            progress = 0;
            interactor.ShowMessage(failedMessage);
            return;
        }

        panel.SetCalibrated(true);
        progress++;

        if (progress < correctOrder.Length)
        {
            interactor.ShowMessage(string.Format(calibratedMessage, progress, correctOrder.Length));
            return;
        }

        IsSolved = true;
        if (objective != null && interactor.Objectives != null)
            interactor.Objectives.Complete(objective);
        interactor.ShowMessage(solvedMessage);
    }
}
