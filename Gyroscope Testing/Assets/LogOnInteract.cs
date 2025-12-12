using System.Collections.Generic;
using UnityEngine;

public class LogOnInteract : MonoBehaviour
{
    [Header("DB Log Settings")]
    public string actionName = "light_color";
    public string targetNameOverride = null;
    public string userId = null;

    [Header("Color Behavior")]
    [Tooltip("If -1, choose randomly. If 0/1/2, always log that color.")]
    [Range(-1, 2)]
    public int fixedColorIndex = -1;

    public void Interact()
    {
        if (ActionLoggerDB.Instance == null)
        {
            Debug.LogWarning("ActionLoggerDB not found in scene. Add it to a GameObject once.");
            return;
        }

        string target = string.IsNullOrEmpty(targetNameOverride) ? gameObject.name : targetNameOverride;
        int colorIdx = (fixedColorIndex >= 0) ? fixedColorIndex : Random.Range(0, 3);

        var meta = new Dictionary<string, string>
        {
            { "tag", gameObject.tag },
            { "layer", LayerMask.LayerToName(gameObject.layer) }
        };

        ActionLoggerDB.Instance.LogAction(
            action: actionName,
            target: target,
            userId: userId,
            colorIndex: colorIdx,
            meta: meta
        );
    }
}
