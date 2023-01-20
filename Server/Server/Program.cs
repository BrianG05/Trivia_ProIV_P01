using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Int32 port = 8888;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            TcpListener serverSocket = new TcpListener(localAddr, port);

            TcpClient clientSocket = default(TcpClient);
            serverSocket.Start();

            Console.WriteLine(">> Servidor Iniciado");

            ClientControl clientControl= new ClientControl();

            int numPlayer = 0;

            while (true)
            {
                /*
                    List_Players == Lista de clientes, se guarda su socket 
                    esta lista es static para ser compartida por todas las instancias
                */
                clientSocket = serverSocket.AcceptTcpClient();
                ClientControl.List_Players.Add(clientSocket);
                numPlayer++;
                clientControl.AddPlayersName(numPlayer);
            }
        }
    }

    public class ClientControl
    {
        public static List<TcpClient> List_Players = new List<TcpClient>();

        private byte[] inData = new byte[64];
        private byte[] outData = new byte[64];

        private static bool StarGame = false;

        private static void outMsg(TcpClient client, string message)
        {
            byte[] outData = new byte[64];
            NetworkStream networkStream = client.GetStream();
            outData = Encoding.ASCII.GetBytes(message);
            networkStream.Write(outData, 0, outData.Length);
            networkStream.Flush();
        }

        private static string inMsg(TcpClient client)
        {
            byte[] inData = new byte[64];

            NetworkStream networkStream = client.GetStream();
            networkStream.Read(inData, 0, 64);
            string clientData = Encoding.ASCII.GetString(inData);

            return clientData;
        }


        public void AddPlayersName(int numPlayer)
        {
            TcpClient client = List_Players[numPlayer-1];
            NetworkStream networkStream = client.GetStream();

            outMsg(client, "<<Ingresa tu nombre: ");
            string clientData = inMsg(client);

            Console.WriteLine(">>Conexion exitosa con: " + clientData);

            //WaitingPlayers(numPlayer);
        }

        public void WaitingPlayers(int numPlayer) 
        {
            if (numPlayer == 1)
            {
                TcpClient client = List_Players[0];
                NetworkStream networkStream = client.GetStream();

                string ServerRequest = "<<Ingresa enter para empezar juego: ";
                outData = Encoding.ASCII.GetBytes(ServerRequest);
                networkStream.Write(outData, 0, outData.Length);
                networkStream.Flush();

                networkStream.Read(inData, 0, 64);
                string clientData = Encoding.ASCII.GetString(inData);

            }
            else
            {
                while (StarGame = false)
                {

                }
            }
        }

    }
}
