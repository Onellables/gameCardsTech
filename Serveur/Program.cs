using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

namespace ServerApplication
{
    public class Server
    {
        private Connection      lastPlayerId;
        private string          lastMessaged;
        private Player[]        allPlayers;
            
        public Connection LastPlayerId { get => lastPlayerId; set => lastPlayerId = value; }
        public string LastMessaged { get => lastMessaged; set => lastMessaged = value; }
        internal Player[] AllPlayers { get => allPlayers; set => allPlayers = value; }

        public Server()
        {
            allPlayers = new Player[4];
        }

        private void Serialise(Connection Id, string message)
        {
            Crypto<string> Key = new Crypto<string>(message);

            Id.SendObject("Message", Key.Serialize());
        }

        public void SendMessageToOne(Connection Id, string message)
        {
            Serialise(Id, message);
        }

        public void SendMessageToAll(string message)
        {
            for (int i = 0; i < 4; ++i)
                Serialise(AllPlayers[i].Id, message);
        }

        public void SendMessageToOther(int player, string message)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (AllPlayers[i] == null)
                    break;
                if (i != player)
                    Serialise(AllPlayers[i].Id, message);
            }
        }

        public void LaunchServer()
        {
            NetworkComms.AppendGlobalIncomingPacketHandler<byte[]>("Message", MessageReceived);
            Connection.StartListening(ConnectionType.TCP, new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0));
            Console.WriteLine("Server listening for TCP connection on:");
            foreach (System.Net.IPEndPoint localEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);
        }

        public void StopServer()
        {
            NetworkComms.Shutdown();
        }

        public void MessageReceived(PacketHeader header, Connection connection, byte[] crypt)
        {
            Crypto<string> Key = Crypto<string>.Deserialize(crypt);

            lastMessaged = Key.GetMessage();
            lastPlayerId = connection;
        }

        private void WaitPlayer()
        {
            for (int i = 0; i != 4;)
            {
                for (int j = 0; j < 4; ++j)
                {
                    if (lastPlayerId == null || 
                        (AllPlayers[j] != null && AllPlayers[j].Id.ConnectionInfo.Equals(lastPlayerId.ConnectionInfo)))
                        break;
                    if (j >= i)
                    {
                        AllPlayers[j] = new Player(lastPlayerId);
                        SendMessageToOne(AllPlayers[j].Id, 
                            "Hello and welcome to the game\nYou are player n°" + (i + 1) 
                            + "\nWe are waiting for " + (3 - i) + " more players.");
                        SendMessageToOther(i, "We are waiting for " + (3 - i) + " more players.");
                        if (i != 3)
                            lastPlayerId = null;
                        ++i;
                    }
                }
            }
            SendMessageToAll("All players are ready.\n");
        }

        private void LaunchGame()
        {
            Rules game = new Rules(this, allPlayers);
        }

        static void Main(string[] args)
        {
            Server server = new Server();

            server.LaunchServer();
            server.WaitPlayer();
            server.LaunchGame();
            server.StopServer();
        }
    }
}