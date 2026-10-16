using UnityEngine;

// One step of a scene's mission ("Find the bolt cutters"). Each objective is an asset in
// _Project/Data/Objectives; the scene's ObjectiveManager lists them in order, and the
// interactable that finishes a step references its asset.
[CreateAssetMenu(menuName = "Last Light/Objective", fileName = "Obj_")]
public class ObjectiveData : ScriptableObject
{
    [Tooltip("Shown in the HUD checklist, e.g. \"Find the bolt cutters (north-west catwalk)\".")]
    [TextArea]
    [SerializeField] string text;

    public string Text => text;
}
