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
using System.IO;

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

        private static SortedList sList_Player_Score = new SortedList();

        private int StartGameFirstPlayer = 2;


        private Thread SendQuestionClient;

        /*
             Esta ruta es dada como respuesta por parte del cliente
        */
        private static string AbsolutePath;

        private static SortedList<string, string> sList_Question_Answer = new SortedList<string, string>();

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
            sList_Player_Score.Add(clientData, 0);

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

                Console.WriteLine("\n>>CONTROL RUTA ABSOLUTA ARCHIVO: ");
                Console.WriteLine(clientData);
                Console.WriteLine("\n");

                AbsolutePath = clientData;

                //Se obtienen las preguntas y respuestas para ser guardadadas en List_Questions
                GetQuestions();

                Console.WriteLine("\n>>CONTROL PREGUNTAS ARCHIVO: ");

                foreach (Object i in sList_Question_Answer)
                {
                    Console.WriteLine("Pregunta: " + i);
                }


                Console.WriteLine("\n");
      

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

            StartTrivia();
        }

        private void StartTrivia()
        {
            //Hilo declarado como atributo
            //La trivia se maneja simultaneamente por hilos
            int c = 0;
            foreach (TcpClient client in List_Players)
            {
                c++;
                SendQuestionClient = new Thread(() => SendQuestions(client, c));
                SendQuestionClient.Start();
            }

            Console.WriteLine(">>Se han recorrido todos los clientes");
        }

        private void SendQuestions(TcpClient client, int c)
        {
            for(int i = 0; i < sList_Question_Answer.Count;i++)
            {
                string pregunta = sList_Question_Answer.Keys[i];
                outMsg(client, pregunta);


                string RespuestaCliente = inMsg(client);

                Console.WriteLine(Convert.ToString(c) + ": "+ RespuestaCliente);

                string RespuestaPregunta;

                //Se valida la pregunta obteniendo su valor (Asociado a esa llave) y comparando con la respuesta del cliente

                if (sList_Question_Answer.TryGetValue(pregunta, out RespuestaPregunta))
                {
                    if (RespuestaPregunta.Equals(RespuestaCliente))
                    {
                        Console.WriteLine("Respuesta correcta!");
                    }
                    else
                    {
                        Console.WriteLine("Respuesta incorrecta!");
                    }
                }
            }

            outMsg(client, "1");

            string res = inMsg(client);
        }

        private void GetQuestions()
        {
            StreamReader reader = new StreamReader(AbsolutePath);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] elementos = line.Split(';');

                //Se guardan las preguntas y respuestas la tabla hash con los elementos del vector elementos
                sList_Question_Answer.Add(elementos[0], elementos[1]);
            }
        }

        //Para eliminar el archivo una vez terminada la partida
        private void DeleteQuestionsFile()
        {
            if (File.Exists(AbsolutePath))
            {
                File.Delete(AbsolutePath);
                Console.WriteLine(">>El archivo ha sido eliminado");
            }
            else
                Console.WriteLine(">>El archivo no existe");
        }

    }
}
