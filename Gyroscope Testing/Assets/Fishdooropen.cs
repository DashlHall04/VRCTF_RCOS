using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FishUnlockTrigger : MonoBehaviour
{
    [Header("Correct Fish")]
    public string correctFishTag = "CorrectFish";

    [Header("Door")]
    public DoorController door;

    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;

        if (other.CompareTag(correctFishTag))
        {
            _triggered = true;
            door.OpenDoor();
        }
    }
}
