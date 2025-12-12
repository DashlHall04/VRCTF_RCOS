using UnityEngine;

public class FishDoorUnlocker : MonoBehaviour
{
    [Header("References")]
    public FishingController fishing;
    public DoorController door;

    [Header("Correct Fish Rule")]
    [Tooltip("If set, caught fish must have this Tag to open the door.")]
    public string correctFishTag = "CorrectFish";

    [Tooltip("Optional: if set, caught fish name must contain this text (e.g., 'GoldenFish').")]
    public string correctFishNameContains = "";

    private bool _unlocked;

    private void OnEnable()
    {
        if (fishing != null)
            fishing.OnFishCaught += HandleFishCaught;
    }

    private void OnDisable()
    {
        if (fishing != null)
            fishing.OnFishCaught -= HandleFishCaught;
    }

    private void HandleFishCaught(GameObject fish)
    {
        if (_unlocked || fish == null) return;

        bool ok = true;

        if (!string.IsNullOrEmpty(correctFishTag))
            ok &= fish.CompareTag(correctFishTag);

        if (!string.IsNullOrEmpty(correctFishNameContains))
            ok &= fish.name.Contains(correctFishNameContains);

        if (!ok) return;

        _unlocked = true;
        door.OpenDoor();
    }
}