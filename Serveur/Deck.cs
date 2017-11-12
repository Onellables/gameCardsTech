using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

namespace ServerApplication
{
    public class Card
    {
        private string face;
        private string color;

        public string Face { get => face; set => face = value; }
        public string Color { get => color; set => color = value; }

        public Card(string Face, string Color)
        {
            face = Face;
            color = Color;
        }
        
        public string SendValue()
        {
            if (face == "")
                return (color);
            return (face + " of " + color);
        }
    }

    public class Hand
    {
        private int nbrCard;
        private Card[] hand;

        public int NbrCard { get => nbrCard; set => nbrCard = value; }

        public Card[] GetHand() { return hand; }
        public void SetHand(Card[] value) { hand = value; }

        public Hand(Deck deck)
        {
            NbrCard = 7;
            NewHand(deck);
        }

        private void NewHand(Deck deck)
        {
            hand = new Card[NbrCard];
            for (int i = 0; i < nbrCard; ++i)
                hand[i] = deck.Deal();
        }

        public void ShowHand(Connection Id, Server serveur, Rules game)
        {
            serveur.SendMessageToOne(Id, "You actually have " + nbrCard + " Cards :\n\n");
            for (int i = 0; i < nbrCard; i++)
            {
                serveur.SendMessageToOne(Id, i + " -\t" + hand[i].SendValue() + game.CardHelp(hand[i]));
                System.Threading.Thread.Sleep(100);
            }
            System.Threading.Thread.Sleep(100);
        }

        public void AddInHand(Deck deck, int nbr, Server server, Player player)
        {
            int idx;

            nbrCard = nbrCard + nbr;
            if (nbrCard > hand.Length)
            {
                Card[] newHand = new Card[NbrCard];
                int handSize = hand.Length;

                idx = 0;
                while (idx < handSize)
                {
                    newHand[idx] = hand[idx];
                    idx = idx + 1;
                }
                while (idx < nbrCard)
                {
                    newHand[idx] = deck.Deal();
                    server.SendMessageToOne(player.Id, "You draw a " + newHand[idx].SendValue());
                    idx = idx + 1;
                }
                SetHand(newHand);
            }
            else
            {
                idx = nbrCard - nbr;
                while (idx < nbrCard)
                {
                    hand[idx] = deck.Deal();
                    server.SendMessageToOne(player.Id, "You draw a " + hand[idx].SendValue());
                    idx = idx + 1;
                }
            }
        }

        public void ThrowCard(int i)
        {
            nbrCard = nbrCard - 1;
            while (i < nbrCard && i < hand.Length)
            {
                hand[i] = hand[i + 1];
                i = i + 1;
            }
        }

    }

    public class Deck
    {
        private Card[]  deck;
        private Random  rand;
        private int     idx;

        public Random Nbr { get => rand; set => rand = value; }
        public int Idx { get => idx; set => idx = value; }

        public string[] Color = { "Hearts", "Clubs", "Spades", "Diamonds", "Joker" };
        public string[] Faces = { "Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight",
                    "Nine", "Ten", "Jack", "Queen", "King" };

        public Deck()
        {
            deck = new Card[54];
            rand = new Random();
            idx = 0;
            for (int i = 0; i < 52; ++i)
                deck[i] = new Card(Faces[i % 13], Color[i / 13]);
            deck[52] = new Card("", "Joker");
            deck[53] = new Card("", "Joker");
        }

        public Card[] GetDeck() { return deck; }
        public void SetDeck(Card[] value) { deck = value; }

        public void Shuffle()
        {
            idx = 0;
            for (int i = 0; i < deck.Length; ++i)
            {
                int temp = rand.Next(deck.Length);
                Card swap = deck[temp];
                deck[temp] = deck[i];
                deck[i] = swap;
            }
        }

        public Card Deal()
        {
            idx = idx + 1;
            if (idx - 1 < deck.Length)
                return (deck[idx - 1]);
            return (null);
        }
    }
}
