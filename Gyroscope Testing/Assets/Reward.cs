using UnityEngine;
using UnityEngine.Events;

namespace Game.Poker
{
    public class PokerRewardSystem : MonoBehaviour
    {
        [Header("References")]
        public PokerTableBank tableBank;

        [Header("Rewards")]
        public int bonusChipsOnWin = 250;
        public string rewardedWinnerId = "P1";

        [Header("Door Reward (optional)")]
        public GameObject doorObject; // if you want to enable/disable, or you can use your DoorController
        public MonoBehaviour doorController; // drag your DoorController here if you have one

        [Header("Spawn Reward (optional)")]
        public GameObject rewardPrefab;
        public Transform rewardSpawnPoint;

        [Header("Events")]
        public UnityEvent OnRewardTriggered;

        private bool _rewarded;

        /// <summary>
        /// Call this from your game flow when you determine the winner.
        /// (Hand evaluation not included; you decide who wins.)
        /// </summary>
        public void HandleWinner(string winnerPlayerId)
        {
            if (_rewarded) return;

            // Always payout pot through the bank (normal poker flow)
            tableBank.PayoutWinner(winnerPlayerId);

            // Reward condition
            if (winnerPlayerId != rewardedWinnerId) return;

            _rewarded = true;

            // Bonus chips
            var p = tableBank.GetPlayer(winnerPlayerId);
            if (p != null)
            {
                p.stack += bonusChipsOnWin;
                // manually notify (since we updated stack directly)
                // simplest: call StartNewHand() soon or add a public notifier; keep minimal here.
            }

            // Door open (works with your DoorController if assigned)
            if (doorController != null)
            {
                // Try call OpenDoor() by name (no hard dependency)
                doorController.Invoke("OpenDoor", 0f);
            }
            else if (doorObject != null)
            {
                doorObject.SetActive(false); // e.g. "remove" the door
            }

            // Spawn reward object
            if (rewardPrefab != null && rewardSpawnPoint != null)
                Instantiate(rewardPrefab, rewardSpawnPoint.position, rewardSpawnPoint.rotation);

            OnRewardTriggered?.Invoke();
        }

        public void ResetRewards()
        {
            _rewarded = false;
        }
    }
}