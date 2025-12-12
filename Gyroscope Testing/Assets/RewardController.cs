using System.Collections;
using UnityEngine;

public class RewardController : MonoBehaviour
{
    [Header("Vase Movement")]
    public Transform vase;
    public float moveDistanceX = 0.5f;
    public float moveDistanceY = 1.5f;
    public float moveDuration = 1.0f;

    [Header("Lights")]
    public LightPatternPlayer lightPlayer;

    private bool _rewardTriggered = false;

    public void TriggerReward(int colorIndex)
    {
        if (_rewardTriggered) return;
        _rewardTriggered = true;

        // Turn lights off
        if (lightPlayer != null)
            lightPlayer.TurnOffLights();

        // Randomize movement based on color
        Vector3 offset = GetRandomizedOffset(colorIndex);
        StartCoroutine(MoveVase(offset));
    }

    private Vector3 GetRandomizedOffset(int colorIndex)
    {
        float x = Random.Range(0.2f, moveDistanceX);
        float y = Random.Range(1.0f, moveDistanceY);

        // Slight variation based on color index
        if (colorIndex == 1) x *= -1f;
        if (colorIndex == 2) y *= 1.2f;

        return new Vector3(x, y, 0f);
    }

    private IEnumerator MoveVase(Vector3 offset)
    {
        Vector3 start = vase.position;
        Vector3 end = start + offset;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            vase.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        vase.position = end;
    }
}
