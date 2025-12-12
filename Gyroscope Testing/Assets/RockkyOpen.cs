using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Movement")]
    public Vector3 openOffset = new Vector3(0f, 3f, 0f);
    public float openDuration = 1.2f;

    private Vector3 _closedPos;
    private bool _isOpen = false;

    private void Awake()
    {
        _closedPos = transform.position;
    }

    public void OpenDoor()
    {
        if (_isOpen) return;
        _isOpen = true;

        StopAllCoroutines();
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Vector3 start = _closedPos;
        Vector3 end = _closedPos + openOffset;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / openDuration;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
    }
}
