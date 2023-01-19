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

            while (true)
            {
                byte[] inData = new byte[64];
                byte[] outData = new byte[64];

                clientSocket = serverSocket.AcceptTcpClient();
                NetworkStream networkStream = clientSocket.GetStream();

                string RespuestaServer = ">>Ingresa tu nombre: ";
                outData = Encoding.ASCII.GetBytes(RespuestaServer);
                networkStream.Write(outData, 0, outData.Length);
                networkStream.Flush();

                networkStream.Read(inData, 0, 64);
                string clientData = Encoding.ASCII.GetString(inData);
                //string NombreCliente = DatosDelCliente.Substring(0, DatosDelCliente.IndexOf("$"));

                Console.WriteLine(">>Conexion exitosa con "+clientData);

                Thread ThreadPlayer = new Thread(() => clientControl.StartGame(networkStream));
                ThreadPlayer.Start();
            }
        }
    }

    public class ClientControl
    {
        public void StartGame(NetworkStream networkStream)
        {

        }

    }
}
