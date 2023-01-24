using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using System.Collections;

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

                /*
                       AddPlayersName representa el metodo de inicio del juego
                */
                Thread threadPlayer = new Thread(() => clientControl.AddPlayersName(numPlayer));
                threadPlayer.Start();
            }
        }
    }

    public class ClientControl
    {
        public static List<TcpClient> List_Players = new List<TcpClient>();

        private static Hashtable List_Player_Score = new Hashtable();

        private int StartGameFirstPlayer = 2;

        //Para enviar datos al cliente
        private static void outMsg(TcpClient client, string message)
        {
            byte[] outData = new byte[256];
            NetworkStream networkStream = client.GetStream();
            outData = Encoding.ASCII.GetBytes(message);
            networkStream.Write(outData, 0, outData.Length);
            networkStream.Flush();
        }

        //Para recibir datos del cliente
        private static string inMsg(TcpClient client)
        {
            byte[] inData = new byte[256];

            NetworkStream networkStream = client.GetStream();
            networkStream.Read(inData, 0, 256);
            string clientData = Encoding.ASCII.GetString(inData);
            clientData = clientData.Substring(0, clientData.IndexOf("$"));

            return clientData;
        }

        public void AddPlayersName(int numPlayer)
        {
            TcpClient client = List_Players[numPlayer-1];
            NetworkStream networkStream = client.GetStream();

            outMsg(client, "<<Ingresa tu nombre:$");
            string clientData = inMsg(client);

            //Nombre jugador - Puntaje
            List_Player_Score.Add(clientData, 0);

            Console.WriteLine(">>Conexion exitosa con: " + clientData);

            WaitingPlayers(numPlayer);

        }

        private void WaitingPlayers(int numPlayer) 
        {
            if (numPlayer == 1)
            {
                TcpClient client = List_Players[0];
                outMsg(client, "1$");

                string clientData = inMsg(client);

                Console.WriteLine(clientData);

                /*
                    StartGameFirstPlayer es una variable de apoyo para que, en los otros clientes y por el lado del cliente se
                    terminen sus bucles while. Esta variable solo es alterada por el primer jugador que se conecto
                */
                StartGameFirstPlayer = 3;
            }
            else
            {
                int i = 1;
                while (true)
                {
          
                    if (i == List_Players.Count)
                    {
                        i = 1;
                    }
                    else
                    {
                        outMsg(List_Players[i], Convert.ToString(StartGameFirstPlayer) + "$");

                        string clientData = inMsg(List_Players[i]);

                        /*
                            Si el valor StartGameFirstPlayer envia un 3 a el cliente este mismo regresa otro 3 para terminar bucle
                        */
                        if (clientData == "3")
                        {
                            break;
                        }

                        i++;
                    }
                }
            }
        }

    }
}
