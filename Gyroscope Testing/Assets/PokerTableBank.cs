using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Poker
{
    [Serializable]
    public class PlayerBank
    {
        public string playerId;
        public int stack;         // chips remaining
        public int currentBet;    // chips committed this betting round
        public bool isFolded;
        public bool isAllIn;

        public PlayerBank(string id, int startingStack)
        {
            playerId = id;
            stack = startingStack;
            currentBet = 0;
            isFolded = false;
            isAllIn = false;
        }
    }

    /// <summary>
    /// "Databank" for table money flow: stacks, bets, pot, blinds, payouts.
    /// (No hand evaluation here; feed winners in externally.)
    /// </summary>
    public class PokerTableBank : MonoBehaviour
    {
        [Header("Players")]
        [Tooltip("Set up player ids in inspector or via code.")]
        public List<string> playerIds = new List<string> { "P1", "P2" };
        public int startingStack = 1000;

        [Header("Blinds")]
        public int smallBlind = 10;
        public int bigBlind = 20;

        public IReadOnlyList<PlayerBank> Players => _players;
        public int Pot { get; private set; }
        public int CurrentToCall { get; private set; }

        public event Action<int> OnPotChanged;
        public event Action<string, int> OnStackChanged; // (playerId, newStack)
        public event Action<string, int> OnBetChanged;   // (playerId, newBet)
        public event Action<string> OnPlayerFolded;
        public event Action<string> OnPlayerAllIn;

        private readonly List<PlayerBank> _players = new();

        private void Awake()
        {
            ResetTable();
        }

        public void ResetTable()
        {
            _players.Clear();
            foreach (var id in playerIds)
                _players.Add(new PlayerBank(id, startingStack));

            Pot = 0;
            CurrentToCall = 0;
            OnPotChanged?.Invoke(Pot);
        }

        public PlayerBank GetPlayer(string playerId)
        {
            return _players.Find(p => p.playerId == playerId);
        }

        public void StartNewHand()
        {
            // Reset per-hand state
            Pot = 0;
            CurrentToCall = 0;
            foreach (var p in _players)
            {
                p.currentBet = 0;
                p.isFolded = false;
                p.isAllIn = false;
                OnBetChanged?.Invoke(p.playerId, p.currentBet);
            }
            OnPotChanged?.Invoke(Pot);
        }

        public void PostBlinds(string smallBlindPlayerId, string bigBlindPlayerId)
        {
            Bet(smallBlindPlayerId, smallBlind);
            Bet(bigBlindPlayerId, bigBlind);
            CurrentToCall = bigBlind;
        }

        /// <summary>
        /// Commit chips to the pot for a player (used by call/raise/blinds).
        /// If player doesn't have enough, they go all-in with what they have.
        /// </summary>
        public void Bet(string playerId, int amount)
        {
            var p = GetPlayer(playerId);
            if (p == null || p.isFolded || p.isAllIn) return;
            if (amount <= 0) return;

            int pay = Mathf.Min(amount, p.stack);
            p.stack -= pay;
            p.currentBet += pay;
            Pot += pay;

            if (p.stack == 0)
            {
                p.isAllIn = true;
                OnPlayerAllIn?.Invoke(p.playerId);
            }

            OnStackChanged?.Invoke(p.playerId, p.stack);
            OnBetChanged?.Invoke(p.playerId, p.currentBet);
            OnPotChanged?.Invoke(Pot);

            if (p.currentBet > CurrentToCall)
                CurrentToCall = p.currentBet;
        }

        public void Fold(string playerId)
        {
            var p = GetPlayer(playerId);
            if (p == null || p.isFolded) return;
            p.isFolded = true;
            OnPlayerFolded?.Invoke(p.playerId);
        }

        public int AmountToCall(string playerId)
        {
            var p = GetPlayer(playerId);
            if (p == null) return 0;
            return Mathf.Max(0, CurrentToCall - p.currentBet);
        }

        public void Call(string playerId)
        {
            int toCall = AmountToCall(playerId);
            Bet(playerId, toCall);
        }

        public void RaiseTo(string playerId, int newTotalBet)
        {
            var p = GetPlayer(playerId);
            if (p == null) return;
            if (newTotalBet <= CurrentToCall) return;

            int add = newTotalBet - p.currentBet;
            Bet(playerId, add);
            CurrentToCall = Mathf.Max(CurrentToCall, newTotalBet);
        }

        /// <summary>
        /// Award the entire pot to a single winner.
        /// </summary>
        public void PayoutWinner(string winnerPlayerId)
        {
            var w = GetPlayer(winnerPlayerId);
            if (w == null) return;

            w.stack += Pot;
            Pot = 0;

            OnStackChanged?.Invoke(w.playerId, w.stack);
            OnPotChanged?.Invoke(Pot);
        }

        /// <summary>
        /// Split pot evenly among winners (simple split; remainder goes to first winner).
        /// </summary>
        public void PayoutSplit(List<string> winnerIds)
        {
            if (winnerIds == null || winnerIds.Count == 0) return;

            int split = Pot / winnerIds.Count;
            int remainder = Pot - split * winnerIds.Count;

            for (int i = 0; i < winnerIds.Count; i++)
            {
                var p = GetPlayer(winnerIds[i]);
                if (p == null) continue;

                int add = split + (i == 0 ? remainder : 0);
                p.stack += add;
                OnStackChanged?.Invoke(p.playerId, p.stack);
            }

            Pot = 0;
            OnPotChanged?.Invoke(Pot);
        }
    }
}
