using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Poker
{
    public enum Suit { Clubs, Diamonds, Hearts, Spades }

    [Serializable]
    public struct Card
    {
        public Suit suit;
        public int rank; // 2..14 where 11=J,12=Q,13=K,14=A

        public Card(Suit s, int r)
        {
            suit = s;
            rank = r;
        }

        public override string ToString()
        {
            string r = rank switch
            {
                11 => "J",
                12 => "Q",
                13 => "K",
                14 => "A",
                _ => rank.ToString()
            };
            return $"{r} of {suit}";
        }
    }

    /// <summary>
    /// Deck databank: builds a 52-card deck, shuffles, deals.
    /// </summary>
    public class CardDeck : MonoBehaviour
    {
        public int seed = 0; // 0 = random seed

        private System.Random _rng;
        private List<Card> _deck = new();
        private int _topIndex = 0;

        public event Action OnShuffled;

        private void Awake()
        {
            ResetDeck();
        }

        public void ResetDeck()
        {
            _deck.Clear();
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
            {
                for (int r = 2; r <= 14; r++)
                    _deck.Add(new Card(s, r));
            }
            _topIndex = 0;

            _rng = (seed == 0) ? new System.Random() : new System.Random(seed);
        }

        public void Shuffle()
        {
            // Fisher–Yates
            for (int i = _deck.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
            }
            _topIndex = 0;
            OnShuffled?.Invoke();
        }

        public bool HasCards(int count = 1)
        {
            return _topIndex + count <= _deck.Count;
        }

        public Card DealOne()
        {
            if (!HasCards(1))
                throw new InvalidOperationException("Deck out of cards. Reset + Shuffle.");

            return _deck[_topIndex++];
        }

        public List<Card> DealMany(int count)
        {
            if (!HasCards(count))
                throw new InvalidOperationException("Not enough cards. Reset + Shuffle.");

            var list = new List<Card>(count);
            for (int i = 0; i < count; i++)
                list.Add(DealOne());
            return list;
        }
    }
}