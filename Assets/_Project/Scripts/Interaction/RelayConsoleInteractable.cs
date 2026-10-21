using UnityEngine;

// The Main Relay Console in the tower compound. It works in two stages. First the UV Power
// Core goes in (used up): the compound gets power, the maintenance yard opens and the
// console reports two faults. Then the Signal Amplifier (used up) powers the antenna
// panels. After that, using it shows the array's status.
public class RelayConsoleInteractable : Interactable
{
    [Header("Stage 1: the core")]
    [SerializeField] ItemData core;
    [SerializeField] ObjectiveData coreObjective;
    [SerializeField] string corePrompt = "Install UV Power Core";
    [TextArea]
    [SerializeField] string missingCoreMessage = "The console is dead. It needs a power core.";
    [SerializeField] string reportTitle = "MAIN RELAY CONSOLE";
    [Tooltip("Opens in the reading panel once the core is in.")]
    [TextArea(6, 14)]
    [SerializeField] string faultReport;
    [Tooltip("Opened once the core powers the compound.")]
    [SerializeField] DoorOpener yardGate;

    [Header("Stage 2: the amplifier")]
    [SerializeField] ItemData amplifier;
    [SerializeField] ObjectiveData amplifierObjective;
    [SerializeField] string amplifierPrompt = "Insert Signal Amplifier";
    [TextArea]
    [SerializeField] string missingAmplifierMessage = "Fault 1: no signal amplifier. There's a spare in the maintenance yard.";
    [TextArea]
    [SerializeField] string amplifierInstalledMessage = "Amplifier online. The antenna panels have power.";
    [SerializeField] AntennaPuzzleController antennaPuzzle;

    [Header("After both")]
    [SerializeField] string statusPrompt = "Check console";
    [TextArea(4, 10)]
    [SerializeField] string misalignedStatus;
    [TextArea(4, 10)]
    [SerializeField] string readyStatus;

    bool coreInstalled;
    bool amplifierInstalled;

    public override string Prompt => !coreInstalled ? corePrompt : !amplifierInstalled ? amplifierPrompt : statusPrompt;

    public override void Interact(PlayerInteractor interactor)
    {
        if (!enabled)
            return;

        if (interactor.Inventory == null)
        {
            Debug.LogWarning("RelayConsoleInteractable: no Inventory in the scene. Is the Systems prefab missing?", this);
            return;
        }

        if (!coreInstalled)
            InstallCore(interactor);
        else if (!amplifierInstalled)
            InstallAmplifier(interactor);
        else
            interactor.OpenReading(reportTitle, antennaPuzzle != null && antennaPuzzle.IsSolved ? readyStatus : misalignedStatus);
    }

    void InstallCore(PlayerInteractor interactor)
    {
        if (!interactor.Inventory.RemoveItem(core))
        {
            interactor.ShowMessage(missingCoreMessage);
            return;
        }

        coreInstalled = true;
        if (yardGate != null)
            yardGate.Open();

        CompleteObjective(interactor, coreObjective);
        interactor.OpenReading(reportTitle, faultReport);
    }

    void InstallAmplifier(PlayerInteractor interactor)
    {
        if (!interactor.Inventory.RemoveItem(amplifier))
        {
            interactor.ShowMessage(missingAmplifierMessage);
            return;
        }

        amplifierInstalled = true;
        if (antennaPuzzle != null)
            antennaPuzzle.PowerOn();

        CompleteObjective(interactor, amplifierObjective);
        interactor.ShowMessage(amplifierInstalledMessage);
    }
}
