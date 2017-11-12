using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

namespace ClientApplication
{
    class Client
    {
        private Connection lastPlayerId;
        private string lastMessaged;

        public Connection LastPlayerId { get => lastPlayerId; set => lastPlayerId = value; }
        public string LastMessaged { get => lastMessaged; set => lastMessaged = value; }

        static public void MessageReceived(PacketHeader header, Connection connection, string message)
        {
            if (message == "clear")
                Console.Clear();
            else
                Console.WriteLine(message);
        }

        static void Main(string[] args)
        {
            int i = 0;
            string serverInfo = null;

            while (i == 0)
            {
                Console.WriteLine("Please enter the server IP and port in the format 192.168.0.1:10000 and press return:");
                serverInfo = Console.ReadLine();
                i = 1;
                if (!(serverInfo.Contains(":")) || serverInfo == "")
                {
                    i = 0;
                    Console.WriteLine("You made an Error writing the server IP and Port.\nTry again.");
                }
            }
            string serverIP = serverInfo.Split(':').First();
            int serverPort = int.Parse(serverInfo.Split(':').Last());
            Console.WriteLine("Waiting answer from the server.\nIf you don't have an answer within 30 seconds, the game must be full or the client failed to connect to the server.\n");
            try
            {
                Connection.StartListening(ConnectionType.TCP, new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0));
                NetworkComms.AppendGlobalIncomingPacketHandler<string>("Message", MessageReceived);
                NetworkComms.SendObject("Message", serverIP, serverPort, "ready");
            }
            catch
            {
                Console.WriteLine("You made an Error writing the server IP and Port.");
                return;
            }
            Console.WriteLine("Type \"Quit\" for quit the Game.\n");
            int game = 0;
            while (game == 0)
            {
                string message = Console.ReadLine();
                if (message != "Quit")
                    NetworkComms.SendObject("Message", serverIP, serverPort, message);
                else
                    game = 1;
            }
            Console.WriteLine("Cuting the Communication");
            NetworkComms.Shutdown();
            Console.WriteLine("\tDone.\nThank you for playing.");
        }

    }
}