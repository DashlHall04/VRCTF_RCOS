using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LightPatternPlayer : MonoBehaviour
{
    [Header("Correct Answer")]
public int correctColorIndex = 0;
public RewardController rewardController;
    [Header("Target Renderer")]
    public Renderer targetRenderer;

    [Header("3 Colors (indexes 0/1/2)")]
    public Color color0 = Color.red;
    public Color color1 = Color.green;
    public Color color2 = Color.blue;

    [Header("Pattern Settings")]
    public float stepSeconds = 0.35f;
    public int randomPatternLength = 10;

    [Header("Database Integration")]
    public bool useDatabasePattern = true;
    public string targetNameOverride = null;
    public string dbActionName = "light_color";
    public int dbReadLimit = 12;
    public bool logWhilePlaying = false;
    public string userId = null;

    private MaterialPropertyBlock _mpb;

    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    [ContextMenu("Play Pattern Now")]
    public void PlayPatternNow()
    {
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning("LightPatternPlayer: targetRenderer not set.");
            yield break;
        }

        List<int> pattern = null;

        if (useDatabasePattern && ActionLoggerDB.Instance != null)
        {
            string target = string.IsNullOrEmpty(targetNameOverride) ? gameObject.name : targetNameOverride;
            pattern = ActionLoggerDB.Instance.GetRecentColorPattern(target, dbActionName, dbReadLimit);

            // DB returns newest-first; reverse so it plays in recorded order
            pattern.Reverse();
        }

        if (pattern == null || pattern.Count == 0)
        {
            pattern = GenerateRandomPattern(randomPatternLength);
        }

        foreach (int idx in pattern)
        {
            ApplyColorIndex(idx);

            if (logWhilePlaying && ActionLoggerDB.Instance != null)
            {
                string target = string.IsNullOrEmpty(targetNameOverride) ? gameObject.name : targetNameOverride;
                ActionLoggerDB.Instance.LogAction(
                    action: dbActionName,
                    target: target,
                    userId: userId,
                    colorIndex: idx,
                    meta: null
                );
            }

            yield return new WaitForSeconds(stepSeconds);
        }
    }

    private List<int> GenerateRandomPattern(int length)
    {
        var list = new List<int>(length);
        for (int i = 0; i < length; i++)
            list.Add(Random.Range(0, 3));
        return list;
    }
    public void TurnOffLights()
{
    if (targetRenderer == null) return;

    targetRenderer.GetPropertyBlock(_mpb);
    _mpb.SetColor("_BaseColor", Color.black);
    _mpb.SetColor("_Color", Color.black);
    targetRenderer.SetPropertyBlock(_mpb);
}

    private void ApplyColorIndex(int idx)
    {
        Color c = idx switch
        {
            0 => color0,
            1 => color1,
            2 => color2,
            _ => Color.white
        };

        // Set color via MaterialPropertyBlock (no material instancing spam)
        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_BaseColor", c);   // URP/Lit
        _mpb.SetColor("_Color", c);       // Built-in/Standard fallback
        targetRenderer.SetPropertyBlock(_mpb);
        if (idx == correctColorIndex && rewardController != null)
{
    rewardController.TriggerReward(idx);
}
    }
}

