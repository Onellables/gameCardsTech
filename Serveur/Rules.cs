using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    public class Rules
    {
        private Deck deck;
        private Card lastCard;
        private int round;
        private int end;
        private int turn;
        private char sense;
        private int winner;
        private int winnerScore;

        public Rules(Server server, Player[] allPlayers)
        {
            round = 0;
            Help(server);
            sense = '+';
            System.Threading.Thread.Sleep(10000);
            while (round < 3)
            {
                server.SendMessageToAll("clear");
                System.Threading.Thread.Sleep(2000);
                server.SendMessageToAll("Time for another round.\nHere we Go");
                deck = new Deck();
                deck.Shuffle();
                lastCard = deck.Deal();
                InitPlayersHand(deck, allPlayers);
                end = -1;
                turn = 0;
                System.Threading.Thread.Sleep(1000);
                for (int i = 0; i < 4; i++)
                    allPlayers[i].hand.ShowHand(allPlayers[i].Id, server, this);
                System.Threading.Thread.Sleep(1000);
                while (end == -1 && deck.Idx < 54)
                {
                    end = 2;
                    YourTurn(allPlayers[turn], server, allPlayers);
                    if (sense == '+')
                        turn = turn + 1;
                    else
                        turn = turn - 1;
                    if (turn == -1)
                        turn = 3;
                    else if (turn == 4)
                        turn = 0;
                }
                server.SendMessageToAll("Player n°" + end + " is the winner of this round.");
                for (int i = 0; i < 4; i++)
                {
                    if (i != end)
                        allPlayers[i].Score += GetScore(allPlayers[i].hand);
                    server.SendMessageToOne(allPlayers[i].Id, "Your total score is " + allPlayers[i].Score.ToString());
                }
                round = round + 1;
            }
            server.SendMessageToAll("clear");
            server.SendMessageToAll("Here is the final score :\nPlayer n°1 : " + allPlayers[0].Score.ToString() + "\nPlayer n°2 : " + allPlayers[1].Score.ToString() +
                "\nPlayer n°3 : " + allPlayers[2].Score.ToString() + "\nPlayer n°4 : " + allPlayers[3].Score.ToString());
            winner = 0;
            winnerScore = allPlayers[0].Score;
            for (int i = 0; i < 4; i++)
            {
                if (allPlayers[i].Score < winnerScore)
                {
                    winner = i;
                    winnerScore = allPlayers[i].Score;
                }
            }
            System.Threading.Thread.Sleep(300);
            server.SendMessageToAll("The winner of this game is Player n°" + winner + " with a score of " + allPlayers[winner].Score.ToString() 
                + "\nWell Done. I hope you enjoyed the game.");
            System.Threading.Thread.Sleep(10000);
        }

        private bool IsValidCard(Card cardPlayed)
        {
            if (lastCard.Color == "Joker")
                return (true);
            if (cardPlayed.Face == lastCard.Face)
                return (true);
            else if (cardPlayed.Color == lastCard.Color)
                return (true);
            else if (cardPlayed.Color == "Joker")
                return (true);
            return (false);
        }

        private void IsJoker(Player player, Player[] allPlayers, Server server)
        {
            int i;

            if (sense == '+')
            {
                i = turn + 1;
                if (i == 4)
                    i = 0;
            }
            else
            {
                i = turn - 1;
                if (i == -1)
                    i = 3;
            }
            allPlayers[i].hand.AddInHand(deck, 4, server, allPlayers[i]);
            server.SendMessageToAll("Player n°" + (i + 1) + " drew 4 cards\n");
        }

        private void IsAce(Player player, Player[] allPlayers, Server server)
        {
            if (sense == '+')
                sense = '-';
            else
                sense = '+';
            server.SendMessageToAll("Now the sense of the game changed.");
        }
            private void IsTwo(Player player, Player[] allPlayers, Server server)
        {
            int i;

            if (sense == '+')
            {
                i = turn + 1;
                if (i == 4)
                    i = 0;
            }
            else
            {
                i = turn - 1;
                if (i == -1)
                    i = 3;
            }
            allPlayers[i].hand.AddInHand(deck, 2, server, allPlayers[i]);
            server.SendMessageToAll("Player n°" + (i + 1)+ " drew 2 cards\n");
        }

        private void IsJack(Player player, Player[] allPlayers, Server server)
        {
            if (sense == '+')
            {
                turn = turn + 1;
                if (turn == 4)
                    turn = 0;
            }
            else
            {
                turn = turn - 1;
                if (turn == -1)
                    turn = 3;
            }
            server.SendMessageToAll("The turn of Player n°" + (turn + 1) + " was skipped.\n");
        }

        private void IsEight(Player player, Player[] allPlayer, Server server)
        {
            int valid = 0;

            while (valid == 0)
            {
                server.SendMessageToOne(player.Id, "Which color do you choose ?\nHearts ?\tClubs ?\tSpades ?\tDiamonds ?");
                server.LastMessaged = null;
                valid = 1;
                while (server.LastMessaged == null)
                { }
                if (server.LastMessaged == "Hearts" || server.LastMessaged == "hearts")
                {
                    lastCard.Color = "Hearts";
                    lastCard.Face = "";
                }
                else if (server.LastMessaged == "Clubs" || server.LastMessaged == "clubs")
                {
                    lastCard.Color = "Clubs";
                    lastCard.Face = "";
                }
                else if (server.LastMessaged == "Spades" || server.LastMessaged == "Spades")
                {
                    lastCard.Color = "Spades";
                    lastCard.Face = "";
                }   
                else if (server.LastMessaged == "Diamonds" || server.LastMessaged == "Diamonds")
                {
                    lastCard.Color = "Diamonds";
                    lastCard.Face = "";
                }
                else
                {
                    valid = 0;
                    server.SendMessageToOne(player.Id, "Unknown color ? (Maybe a Typo Error)");
                }
                System.Threading.Thread.Sleep(2000);
            }
            server.SendMessageToAll("The new color is : " + lastCard.Color);
        }

        private void YourTurn(Player player, Server server, Player[] allPlayer)
        {
            int valid = 0;
            int pos = 0;
            Card[] hand = player.hand.GetHand();

            server.SendMessageToOther(turn, "It's Player " + (turn + 1) + " turn.\n");
            while (valid == 0)
            {
                server.SendMessageToOne(player.Id, "clear");
                server.SendMessageToOne(player.Id, "It's your turn. The current card is : " + lastCard.SendValue());
                player.hand.ShowHand(player.Id, server, this);
                server.SendMessageToOne(player.Id, "Choose your card by entering the number to the left of the chosen card.");
                server.LastMessaged = null;
                while (server.LastMessaged == null)
                { }
                if (server.LastMessaged == "draw")
                    valid = 2;
                else
                {
                    pos = int.Parse(server.LastMessaged);
                    if (IsValidCard(hand[pos]))
                        valid = 1;
                    else if (server.LastMessaged == "draw")
                        valid = 2;
                    else
                        server.SendMessageToOne(player.Id, "You can't play this card.");
                }
                System.Threading.Thread.Sleep(2000);
            }
            if (valid == 1)
            {
                server.SendMessageToOne(player.Id, "You throw your " + hand[pos].SendValue());
                server.SendMessageToOther(turn, "Player N°" + (turn + 1) + " throw a " + hand[pos].SendValue());
                lastCard = hand[pos];
                player.hand.ThrowCard(pos);
                if (lastCard.Face == "Eight")
                    IsEight(player, allPlayer, server);
                else if (lastCard.Face == "Jack")
                    IsJack(player, allPlayer, server);
                else if (lastCard.Face == "Ace")
                    IsAce(player, allPlayer, server);
                else if (lastCard.Face == "Two")
                    IsTwo(player, allPlayer, server);
                else if (lastCard.Color == "Joker")
                    IsJoker(player, allPlayer, server);
                if (player.hand.NbrCard == 0)
                    end = turn;
            }
            else if (valid == 2)
            {
                server.SendMessageToOne(player.Id, "You draw a card");
                server.SendMessageToOther(turn, "Player N°" + (turn + 1) + " draw a card");
                player.hand.AddInHand(deck, 1, server, player);
            }
        }
        
        public string CardHelp(Card card)
        {
            if (card.Face == "Eight")
                return ("\t\t(This card allow you to change the color on the next round)");
            else if (card.Face == "Jack")
                return ("\t\t(This card allow you to skip the turn of the next player)");
            else if (card.Face == "Ace")
                return ("\t\t(This card allow you to change the sens of the game)");
            else if (card.Color == "Joker")
                return ("\t\t(This card make the next Player draw 4 cards. She can be used on any other card)");
            else if (card.Color == "Two")
                return ("\t\t(This card make the next Player draw 2 cards.)");
            return ("");
        }

        public int GetScore(Hand hand)
        {
            int score = 0;
            Card[] CardList = hand.GetHand();
            for (int i = 0; i < hand.NbrCard; ++i)
            {
                if (CardList[i].Face == "Three")
                    score = score + 3;
                else if (CardList[i].Face == "Four")
                    score = score + 4;
                else if (CardList[i].Face == "Five")
                    score = score + 5;
                else if (CardList[i].Face == "Six")
                    score = score + 6;
                else if (CardList[i].Face == "Seven")
                    score = score + 7;
                else if (CardList[i].Face == "Ten" || CardList[i].Face == "King" || CardList[i].Face == "Queen")
                    score = score + 10;
                else if (CardList[i].Face == "Jack" || CardList[i].Face == "Ace" || CardList[i].Face == "Two")
                    score = score + 20;
                else if (CardList[i].Face == "Eight" || CardList[i].Color == "Joker")
                    score = score + 50;
            }
            return (score);
        }

        public void Help(Server server)
        {
            server.SendMessageToAll("Hello and welcome in the game \"Crazy Eights\". The rules are the followings\n" +
                "\nThe goal is to be the first player to get rid of all the cards in his hand.\n\n" +
                "The player who is the first to have no cards left wins the game. The winning player collects from each other player the value of the cards remaining in that player’s hand as follows :\n\n" +
                "Each Eight = 50 Points\nEach King, Queen or Jack = 10 Points\nEach ace = 1 Point\nEach other card is the pip value.\n\n" +
                "This is a 3 set Match. The winner is the one who has least of points at the end of the game.\n" +
                "Let's begin. " +
                "Rock and Roll. Go.");
        }

        private void InitPlayersHand(Deck deck, Player[] allPlayers)
        {
            int i = 0;

            while (i < 4)
            {
                Hand hand = new Hand(deck);
                allPlayers[i].hand = hand;
                i = i + 1;
            }
        }
    }
}
