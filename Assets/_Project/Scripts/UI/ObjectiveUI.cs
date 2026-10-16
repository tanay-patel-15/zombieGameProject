using System.Text;
using TMPro;
using UnityEngine;

// The "OBJECTIVES" checklist in the top-left of the HUD. Lists the scene's objectives in
// order: done ones ticked and greyed out, the current one highlighted. Listens to
// ObjectiveManager and hides when the scene has no objectives.
public class ObjectiveUI : MonoBehaviour
{
    [Tooltip("The child panel to show and hide. Not this object, so the listener stays enabled.")]
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text list;
    [Tooltip("LiberationSans SDF has no tick glyph, so the marks are plain ASCII.")]
    [SerializeField] string doneMark = "[x]";
    [SerializeField] string currentMark = "[ ]";
    [SerializeField] string pendingMark = "[ ]";
    [SerializeField] Color doneColor = new Color(1f, 1f, 1f, 0.45f);
    [SerializeField] Color currentColor = new Color(0.8f, 0.65f, 1f);

    readonly StringBuilder text = new StringBuilder();

    void OnEnable()
    {
        ObjectiveManager.Changed += Show;
        Show(ObjectiveManager.Instance);
    }

    void OnDisable()
    {
        ObjectiveManager.Changed -= Show;
    }

    void Show(ObjectiveManager objectives)
    {
        bool visible = objectives != null && objectives.Objectives.Count > 0;
        panel.SetActive(visible);
        if (!visible)
            return;

        string done = ColorUtility.ToHtmlStringRGBA(doneColor);
        string highlight = ColorUtility.ToHtmlStringRGBA(currentColor);
        ObjectiveData current = objectives.Current;

        text.Clear();
        foreach (ObjectiveData objective in objectives.Objectives)
        {
            if (objective == null)
                continue;

            if (text.Length > 0)
                text.Append('\n');

            if (objectives.IsComplete(objective))
                text.Append($"<color=#{done}>{doneMark} <s>{objective.Text}</s></color>");
            else if (objective == current)
                text.Append($"<color=#{highlight}><b>{currentMark} {objective.Text}</b></color>");
            else
                text.Append($"{pendingMark} {objective.Text}");
        }
        list.text = text.ToString();
    }
}
