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
using System.Timers;

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
            /*
                Los clientes son alamcenados en una lista tipo TcpClient.
                Para identificar al cliente que se encuentra en el hilo y enviar o recibir mensajes se emplea del indice de esa lista
                con ayuda del num cliente.

                Por ejemplo, si num player se encuentra en 0, significa que en la posicion 0 de la lista se encuentra el primer cliente
                en conectarse.

                Ademas, la sortedlist Player_Score sigue la misma logica de la lista anterior explicada
            */

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

        private static SortedList<string, int> sList_Player_Score = new SortedList<string, int>();

        private int StartGameFirstPlayer = 2;

        /*
             Esta ruta es dada como respuesta por parte del cliente
        */
        private static string AbsolutePath;

        //Guarda las preguntas en un sortedlist con su respuesta
        private static SortedList<string, string> sList_Question_Answer = new SortedList<string, string>();

        //Determina cuando todos los jugadores terminaron su trivia
        private static bool FinishTrivia = false;

        //Se guarda la cantidad de jugadores que van finalizando
        private static int FinishTriviaPlayers= 0;

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
            //Se resta 1 para guardar el index en List_Players
            int IndexClient = (numPlayer - 1);

            TcpClient client = List_Players[IndexClient];
            NetworkStream networkStream = client.GetStream();

            outMsg(client, "<<Ingresa tu nombre:$");
            string clientData = inMsg(client);

            //Nombre jugador - Puntaje
            sList_Player_Score.Add(clientData, 0);

            Console.WriteLine(">>Conexion exitosa con: " + clientData);

            WaitingPlayers(numPlayer, IndexClient);

        }

        private void WaitingPlayers(int numPlayer, int IndexClient) 
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
                //i representa el index del jugador en List_Players
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

            StartTrivia(IndexClient);
        }

        private void StartTrivia(int IndexClient)
        {
            string PlayerName;
            int Score;
            int NewScore;
            string Request;
            for (int i = 0; i < sList_Question_Answer.Count; i++)
            {
                string pregunta = sList_Question_Answer.Keys[i];
                outMsg(List_Players[IndexClient], pregunta);

                string RespuestaCliente = inMsg(List_Players[IndexClient]);

                string RespuestaPregunta;
                
                //Se obtiene y valida la respuesta de la pregunta asociada (Valor - Clave)
                if (sList_Question_Answer.TryGetValue(pregunta, out RespuestaPregunta))
                {
                    if (RespuestaPregunta.Equals(RespuestaCliente))
                    {
                        PlayerName = sList_Player_Score.Keys[IndexClient];
                        
                        //Se obtiene el puntaje actual
                        sList_Player_Score.TryGetValue(PlayerName, out Score);

                        //Se suma el puntaje actual
                        sList_Player_Score[PlayerName] = Score + 1;

                        //Obtenemos el nuevo puntaje
                        sList_Player_Score.TryGetValue(PlayerName, out NewScore);

                        Request = "Respuesta correcta! | Puntaje: "+ Convert.ToString(NewScore);
                        outMsg(List_Players[IndexClient], Request);

                        //Se recibe un msj pero no se emplea para nada
                        string r = inMsg(List_Players[IndexClient]);

                        //Console.WriteLine(PlayerName + "CORRECTO!" + Convert.ToString(NewScore));
                    }
                    else
                    {
                        PlayerName = sList_Player_Score.Keys[IndexClient];

                        sList_Player_Score.TryGetValue(PlayerName, out Score);

                        //Se resta el puntaje actual
                        sList_Player_Score[PlayerName] = Score - 1;

                        //Obtenemos el nuevo puntaje
                        sList_Player_Score.TryGetValue(PlayerName, out NewScore);


                        Request = "Respuesta Incorrecta! | Solución: "+ RespuestaPregunta + " | Puntaje: " + Convert.ToString(NewScore);
                        outMsg(List_Players[IndexClient], Request);

                        //Se recibe un msj pero no se emplea para nada
                        string r = inMsg(List_Players[IndexClient]);

                        //Console.WriteLine(PlayerName + " INCORRECTO " + Convert.ToString(NewScore));
                    }
                }
            }

            outMsg(List_Players[IndexClient], "1");

            string res = inMsg(List_Players[IndexClient]);

            Console.WriteLine(">>Client: "+Convert.ToString(IndexClient) +" ha terminado la trivia");
            FinishTriviaPlayers++;

            //Esta funcion "Atrapa" a los clientes hasta que todos los demas hayan finalizado
            while(FinishTrivia != true)
            {
                //Se manda como mensaje el estado de FinishTrivia, es decir, falso o verdadero
                outMsg(List_Players[IndexClient], Convert.ToString(FinishTrivia));

                string requestClient = inMsg(List_Players[IndexClient]);

                if (FinishTriviaPlayers == List_Players.Count)
                {
                    FinishTrivia = true;
                }
            }
            //Cuando FinishTrivia cambie a tru no entra en el bucle anterior, envia un true y rompe el ciclo que se encuentra en el cliente
            outMsg(List_Players[IndexClient], Convert.ToString(FinishTrivia));

            Console.WriteLine("Ah finalizado el juego");

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
