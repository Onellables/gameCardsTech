using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

namespace ServerApplication
{
    public class Player
    {
        private Connection  id;
        public  Hand        hand;
        private int         score;

        public Player(Connection channel)
        {
            score = 0;
            id = channel;
        }


        public Connection Id { get => id; set => id = value; }
        public int Score { get => score; set => score = value; }
    }
}
