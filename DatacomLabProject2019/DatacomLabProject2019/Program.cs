using DatacomLabProject2019;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DatacomLabProject2019
{
    class Client
    {
        public Client(Socket socket)
        {
            Socket = socket;
        }

        public bool IsClientAnswered { get; set; } = false;
        public Socket Socket { get; set; }
        public bool IsClientInTheGame { get; set; } = true;
        public bool IsAwaitingNewQuestion { get; set; }
    }

    static class Contest
    {
        public static List<Question> Questions { get; set; } = new List<Question>();
        public static bool IsGameStarted { get; set; }
        static bool hasQuestionIndexIncreased = false;

        static int questionIndex = 0;

        public static Question GetCurrentQuestion()
        {
            while (true)
            {
                bool allClientsAwaitingNewFunction = true;
                
                foreach (Client client in Clients)
                {
                    if (client.IsClientInTheGame && !client.IsAwaitingNewQuestion)
                    {
                        allClientsAwaitingNewFunction = false;
                    }
                }

                if (allClientsAwaitingNewFunction)
                {
                    break;
                }
            }

            hasQuestionIndexIncreased = false;
            return Questions[questionIndex];
        }

        public static List<Client> Clients { get; set; } = new List<Client>();

        public static void WaitForAllClientAnswers()
        {
            while (true)
            {
                bool allClientsAnswered = true;

                int index = 0;

                foreach (Client client in Clients)
                {
                    index++;

                    if (client.IsClientInTheGame)
                    {

                        if (!client.IsClientAnswered)
                        {

                            allClientsAnswered = false;
                        }
                    }
                }

                if (allClientsAnswered)
                {

                    break;
                }
            }

            if (!hasQuestionIndexIncreased)
            {
                questionIndex++;
                hasQuestionIndexIncreased = true;
            }
        }

        public static void WaitForContestStart()
        {
            while (true)
            {
                
                if (Clients.Count == 4)               //   !!!  client count  !!! yarışmacı sayısı
                {
                    break;
                }
            }
        }

        public static bool CheckIfClientWon()
        {
            Thread.Sleep(10);
            int remainingClientCount = 0;

            foreach (Client clientItem in Clients)
            {
                if (clientItem.IsClientInTheGame)
                {
                    remainingClientCount++;
                }
            }

            if (remainingClientCount == 1)
            {
                return true;
            }

            return false;
        }
    }

    class Program
    {
        static public void SendMessageToClient(Client client, string message)
        {
            client.Socket.Send(Encoding.ASCII.GetBytes(message));
        }

        static public string ReadMessageFromClient(Client client)
        {
            string receivedMessage = string.Empty;

            while (true)
            {
                byte[] receivedBytes = new Byte[1024];
                int byteCount = client.Socket.Receive(receivedBytes);

                receivedMessage += Encoding.ASCII.GetString(receivedBytes, 0, byteCount);

                if (receivedMessage.IndexOf("<EOF>") > -1)
                {
                    break;
                }
            }

            receivedMessage = receivedMessage.Remove(receivedMessage.Length - 5);

            client.IsClientAnswered = true;

            return receivedMessage;
        }

        static public void ClientProcess(Client client)
        {
            Contest.WaitForContestStart();

            while (true)
            {
                client.IsAwaitingNewQuestion = true;
                Question currentQuestion = Contest.GetCurrentQuestion();
                Thread.Sleep(10);
                client.IsAwaitingNewQuestion = false;

                SendMessageToClient(client, currentQuestion.Text);
                client.IsClientAnswered = false;

                string clientsAnswer = ReadMessageFromClient(client);

                if (clientsAnswer == currentQuestion.Answer)
                {
                    SendMessageToClient(client, "correct");
                }
                else
                {
                    SendMessageToClient(client, "incorrect");

                    break;
                }

                Contest.WaitForAllClientAnswers();

                if (Contest.CheckIfClientWon())
                {
                    SendMessageToClient(client, ">>> CONGRATULATIONS!!! YOU WON THE CONTEST.");
                    break;
                }
            }

            client.IsClientInTheGame = false;
        }

        static public void Main(string[] args)
        {
            ContestContext context = new ContestContext();

            Contest.Questions = context.Questions.ToList();

            IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = iPHostEntry.AddressList[0];
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 41849);

            Socket serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                serverSocket.Bind(iPEndPoint);
                serverSocket.Listen(10);

                while (true)
                {
                    Console.WriteLine(">>> WAITING NEW PLAYERS...( NEW CONNECTIONS )");

                    Socket clientSocket = serverSocket.Accept();

                    Console.WriteLine(">>> A NEW PLAYER CONNECTED.");

                    Client client = new Client(clientSocket);

                    Contest.Clients.Add(client);

                    Thread thread = new Thread(() => ClientProcess(client));
                    thread.Start();                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
