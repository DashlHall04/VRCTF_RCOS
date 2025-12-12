using System.Collections;
using UnityEngine;

public class FishingController : MonoBehaviour
{
    [Header("Input")]
    public KeyCode fishKey = KeyCode.E;

    [Header("Casting")]
    public Camera cam;
    public float castRange = 25f;
    public LayerMask waterMask;

    [Header("Fishing Timing")]
    public float minBiteTime = 1.5f;
    public float maxBiteTime = 4.5f;
    public float biteWindowSeconds = 1.0f;   // time to press again to catch
    public float cooldownSeconds = 0.8f;

    [Header("Catch")]
    public Transform catchSpawnPoint;
    public GameObject[] fishPrefabs;

    [Header("Audio (optional)")]
    public AudioSource audioSource;
    public AudioClip castClip;
    public AudioClip biteClip;
    public AudioClip catchClip;
    public AudioClip failClip;

    private bool _isFishing;
    private bool _hasBite;
    private float _biteDeadline;
    private float _cooldownUntil;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (Time.time < _cooldownUntil) return;

        if (Input.GetKeyDown(fishKey))
        {
            if (!_isFishing)
            {
                TryCast();
            }
            else
            {
                TryCatch();
            }
        }

        // Bite window expired
        if (_hasBite && Time.time > _biteDeadline)
        {
            FailCatch();
        }
    }

    private void TryCast()
    {
        if (cam == null) return;

        // Must be aiming at water
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, castRange, waterMask))
        {
            Play(failClip);
            return;
        }

        _isFishing = true;
        _hasBite = false;

        Play(castClip);
        StartCoroutine(BiteRoutine());
    }

    private IEnumerator BiteRoutine()
    {
        // Wait random time until bite
        float wait = Random.Range(minBiteTime, maxBiteTime);
        yield return new WaitForSeconds(wait);

        // Bite happens
        _hasBite = true;
        _biteDeadline = Time.time + biteWindowSeconds;
        Play(biteClip);
    }

    private void TryCatch()
    {
        if (!_hasBite)
        {
            // Reeled too early
            Play(failClip);
            ResetFishingState(cooldownSeconds);
            return;
        }

        // Success
        SpawnCaughtFish();
        Play(catchClip);
        ResetFishingState(cooldownSeconds);
    }

    private void FailCatch()
    {
        Play(failClip);
        ResetFishingState(cooldownSeconds);
    }

    private void ResetFishingState(float cooldown)
    {
        StopAllCoroutines();
        _isFishing = false;
        _hasBite = false;
        _cooldownUntil = Time.time + cooldown;
    }

    private void SpawnCaughtFish()
    {
        if (fishPrefabs == null || fishPrefabs.Length == 0 || catchSpawnPoint == null) return;

        GameObject prefab = fishPrefabs[Random.Range(0, fishPrefabs.Length)];
        GameObject fish = Instantiate(prefab, catchSpawnPoint.position, catchSpawnPoint.rotation);

        // Optional: make it not flop away
        var rb = fish.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Optional: parent to hand
        fish.transform.SetParent(catchSpawnPoint, true);
    }

    private void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}

