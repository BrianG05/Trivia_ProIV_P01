using System;
using System.Collections;
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

            while (true)
            {
                byte[] inData = new byte[64];
                byte[] outData = new byte[64];

                clientSocket = serverSocket.AcceptTcpClient();
                NetworkStream networkStream = clientSocket.GetStream();

                string ServerRequest = ">>Ingresa tu nombre: ";
                outData = Encoding.ASCII.GetBytes(ServerRequest);
                networkStream.Write(outData, 0, outData.Length);
                networkStream.Flush();

                networkStream.Read(inData, 0, 64);
                string clientData = Encoding.ASCII.GetString(inData);

                /*
                    clientPlayer == Identificador de Conexion de cada cliente
                    CountPlayers == Identificar primer jugador
                */
                string clientPlayer = clientData;
                clientControl.CountPlayers++;

                /*
                    ListPlayer = clientPlayer + puntaje
                */
                Console.WriteLine(">>Conexion exitosa con: "+clientData);
                clientControl.ListPlayer.Add(clientPlayer, 0);

                /*
                    NOTA: Obj clientControl como parametro para que todos los jugadores utilicen este objeto
                */
                Thread ThreadPlayer = new Thread(() => clientControl.WaitingPlayers(networkStream, clientControl));
                ThreadPlayer.Start();
            }
        }
    }

    public class ClientControl
    {
        public int CountPlayers = 0;
        public SortedList ListPlayer = new SortedList();
        private bool StartGame = false;


        private string ServerRequest = "";
        private string clientData = "";
        private byte[] inData = new byte[64];
        private byte[] outData = new byte[64];

        public void WaitingPlayers(NetworkStream networkStream, ClientControl clientControl)
        {
            /*
             Identificar primer jugador:
                Si CountPlayers != 1 ya hay mas jugadores conectados
                Si CountPlayers == 1 primer jugador conectado, este decide cuando inicia la trivia 
                Otros jugadores esperan inicio de juego...
            */

            ServerRequest = Convert.ToString(CountPlayers);
            outData = Encoding.ASCII.GetBytes(ServerRequest);
            networkStream.Write(outData, 0, outData.Length);
            networkStream.Flush();

            while (clientControl.StartGame == false)
            {
                networkStream.Read(inData, 0, 64);
                clientData = Encoding.ASCII.GetString(inData);

                if(clientData == "1")
                {
                    break;
                }
            }

        }
    }
}
