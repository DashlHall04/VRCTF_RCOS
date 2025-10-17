using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class ClickMoveAndRecord : MonoBehaviour, IPointerClickHandler
{
    [Header("Bounce Settings")]
    public float moveHeight = 1f;     // how far it moves up
    public float duration = 0.6f;     // total up/down time
    public AnimationCurve easeCurve;  // optional easing curve

    private Vector3 _startPos;
    private bool _animating;

    void Awake()
    {
        _startPos = transform.position;
        if (easeCurve == null || easeCurve.length == 0)
            easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }

    // Called by Unity’s EventSystem when the object is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_animating)
            StartCoroutine(BounceOnce());

        SimpleNameDatabase.AddName(gameObject.name);
        Debug.Log($"Recorded to DB: {gameObject.name} (total={SimpleNameDatabase.Count})");
    }

    private IEnumerator BounceOnce()
    {
        _animating = true;
        float half = duration * 0.5f;

        // Move up
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / half);
            float y = _startPos.y + easeCurve.Evaluate(u) * moveHeight;
            transform.position = new Vector3(_startPos.x, y, _startPos.z);
            yield return null;
        }

        // Move down
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / half);
            float y = _startPos.y + (1f - easeCurve.Evaluate(u)) * moveHeight;
            transform.position = new Vector3(_startPos.x, y, _startPos.z);
            yield return null;
        }

        transform.position = _startPos;
        _animating = false;
    }
}