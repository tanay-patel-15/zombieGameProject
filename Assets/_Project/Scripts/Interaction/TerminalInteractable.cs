using UnityEngine;

// A computer terminal that opens a page of text, like the Warehouse briefing. Reading it
// ticks its objective. It stays usable, so the player can come back and read it again.
public class TerminalInteractable : Interactable
{
    [SerializeField] string title = "TERMINAL";
    [TextArea(6, 14)]
    [SerializeField] string body;

    public override void Interact(PlayerInteractor interactor)
    {
        if (!enabled)
            return;

        interactor.OpenReading(title, body);
        CompleteObjective(interactor);
    }
}
