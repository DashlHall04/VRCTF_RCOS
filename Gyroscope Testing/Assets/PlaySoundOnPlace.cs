using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnPlace : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip placeSound;
    public float minImpactVelocity = 0.1f;

    [Header("Filter")]
    public string requiredTag = ""; // optional: only objects with this tag trigger sound

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Optional tag check
        if (!string.IsNullOrEmpty(requiredTag) && !collision.gameObject.CompareTag(requiredTag))
            return;

        // Make sure it was actually "placed" (not just brushing)
        if (collision.relativeVelocity.magnitude < minImpactVelocity)
            return;

        PlaySound();
    }

    private void PlaySound()
    {
        if (placeSound == null) return;

        _audioSource.PlayOneShot(placeSound);
    }
}
