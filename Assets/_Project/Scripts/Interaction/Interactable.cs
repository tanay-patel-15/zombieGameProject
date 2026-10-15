using UnityEngine;

// Base class for everything the player can use with E. PlayerInteractor focuses the
// nearest one in reach and calls Interact(); each subclass decides what happens.
// Setup: put the script on the object's root and give it a trigger collider on the
// Interactable layer; solid parts stay on Default. Disable the component once the
// object is used up and the player stops focusing it.
public abstract class Interactable : MonoBehaviour
{
    [Tooltip("Shown after the key in the HUD prompt, e.g. \"Use terminal\".")]
    [SerializeField] string prompt = "Interact";

    public virtual string Prompt => prompt;

    public abstract void Interact(PlayerInteractor interactor);
}
