using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cliente_C
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);

            //Socket del cliente

            TcpClient clientSocket = new System.Net.Sockets.TcpClient();

            //Se conecta al server
            clientSocket.Connect(server);

            Client client = new Client();


            string strData = client.inMsg(clientSocket);
            Console.WriteLine(strData);

            string NombreCliente = Console.ReadLine();
            client.outMsg(clientSocket, NombreCliente+"$");

            client.WaitingPlayers(clientSocket);

            client.Trivia(clientSocket);

            //Esperar a que los otros jugadores terminen la trivia
            while (true)
            {
                strData = client.inMsg(clientSocket);

                if(strData == "True")
                {
                    client.outMsg(clientSocket, "$");
                    break;
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Esperando a que los otros jugadores finalicen...");
                    client.outMsg(clientSocket, "continue$");
                }
                Thread.Sleep(1000);
            }

            Console.Clear();
            Console.WriteLine("\n\tJUEGO TERMINADO");
            Console.WriteLine("-------------------------------");

            string Winners = client.inMsg(clientSocket);

            Console.WriteLine(Winners);

            Console.WriteLine("\nPresione enter para finalizar...");
            Console.ReadLine();
            client.outMsg(clientSocket, "delete$");

            clientSocket.Close();

        }
    }
}

public class Client
{   
    //Para enviar datos al server
    public void outMsg(TcpClient clientSocket, string message)
    {
        byte[] outData = new byte[256];
        NetworkStream networkStream = clientSocket.GetStream();
        outData = Encoding.ASCII.GetBytes(message);
        networkStream.Write(outData, 0, outData.Length);
        networkStream.Flush();
    }

    //Para recibir datos del server
    public string inMsg(TcpClient clientSocket)
    {
        byte[] inData = new byte[256];

        NetworkStream networkStream = clientSocket.GetStream();
        networkStream.Read(inData, 0, 256);
        string ServerData = Encoding.ASCII.GetString(inData);
        ServerData = ServerData.Substring(0, ServerData.IndexOf("$"));

        return ServerData;

    }
    public void WaitingPlayers(TcpClient clientSocket)
    {
        while (true)
        {
            string strData = inMsg(clientSocket);

            if(strData == "1")
            {
                CreateQuestionFile(clientSocket);
                Console.Clear();
                Console.WriteLine("El juego ha iniciado! ");
                break;
            }
            else if (strData == "2")
            {
                Console.Clear();
                Console.WriteLine("Esperando jugadores...");
                outMsg(clientSocket, "2$");
            }
            else
            {
                Console.Clear();
                Console.WriteLine("El juego ha iniciado! ");
                outMsg(clientSocket, "3$");
                break;
            }
        }

        Thread.Sleep(1000);
    }

    //Metodo para crear el archivo si es el primer jugador
    private void CreateQuestionFile(TcpClient clientSocket)
    {
        Console.Clear();
        Console.WriteLine("\t CREACIÓN DE PREGUNTAS\n");

        string rutaRelativa = "QuestionFile.txt";

        try
        {
            if (!File.Exists(rutaRelativa))
            {
                string finish = "0";
                Console.Clear();
                using (StreamWriter file = File.CreateText(rutaRelativa))
                {    
                    while (true)
                    {
                        Console.WriteLine("Escribe la pregunta: ");
                        String pregunta = Console.ReadLine();
                        Console.WriteLine("Escribe la respuesta: ");
                        String respuesta = Console.ReadLine();

                        Console.WriteLine("\nIngresa [1] si desea terminar, [0] si desea agregar otra:");
                        Console.WriteLine("-------------------------------");
                        finish = Console.ReadLine();

                        file.WriteLine(pregunta + ";" + respuesta + ";");

                        if (finish == "1")
                        {
                            break;
                        }

                    }
                }

                //Envio de ruta absoluta al servidor
                string rutaAbsoluta = Path.GetFullPath(rutaRelativa);
                outMsg(clientSocket, rutaAbsoluta + "$");
            }




        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al agregar preguntas al archivo: " + ex.Message);
        }
    }

    public void Trivia(TcpClient clientSocket)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("PREGUNTA: ");

            string strData = inMsg(clientSocket);

            if(strData == "1")
            {
                outMsg(clientSocket, "2$");
                break;
            }
            else
            {
                Console.WriteLine(strData + "\n");
                Console.WriteLine("Ingrese la respuesta: ");
                string respuesta = Console.ReadLine();
                outMsg(clientSocket, respuesta + "$");

                string answer = inMsg(clientSocket);
                Console.WriteLine("--------------------------------");
                Console.WriteLine(answer);

                outMsg(clientSocket, "$");
            }

            //Esperar 3 segundos para poder ver la respuesta 
            Thread.Sleep(1000);
        }
    }
}

