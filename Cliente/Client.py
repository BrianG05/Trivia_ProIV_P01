import os
import socket
import time

class ClassPlayer:
    def __init__(self, PlayerName):
        self.PlayerName = PlayerName
    
    def WaitingPlayers(self, ClientSocket):
        #Bloque correspondiente a la espera de los demas clientes
        while(True):
            strData = inMsg(client=ClientSocket)

            if(strData == "1"):
                self.CreateQuestionFile(ClientSocket)
                os.system('cls')
                print("El juego ha iniciado! ")
                break
            elif (strData == "2"):
                os.system('cls')
                print("Esperando Jugadores")
                outMsg(client=ClientSocket,msg="2$")
            else:
                os.system('cls')
                print("El juego ha iniciado! ")
                outMsg(client=ClientSocket,msg="3$")
                break;
            time.sleep(1)
    
    def CreateQuestionFile(self, ClientSocket):
        os.system('cls')
        print("\t CREACIÓN DE PREGUNTAS\n")

        RutaRelativa = "Archivos\QuestionFile.txt"
        Finish = 0
        with open(RutaRelativa, "w") as QuestionFile:
            while(Finish != 1):
                Pregunta = input("Escribe la pregunta: ")
                Respuesta = input("Escribe la respuesta: ")
                Finish = int(input("Ingresa [1] si desea terminar: "))
                print("----------------------------------------------")
                QuestionFile.write(Pregunta + ";"+ Respuesta +";"+"\n")

        QuestionFile.close()
        #Se envia al server la ruta absoluta del archivo para que lo pueda leer
        RutaAbsoluta = os.path.abspath(RutaRelativa)
        outMsg(client=ClientSocket,msg= RutaAbsoluta+"$")

    def Trivia(self,ClientSocket):
        while(True):
            os.system('cls')
            print("PREGUNTA: ")
            strData = inMsg(ClientSocket) 

            if(strData == "1"):
                outMsg(client=ClientSocket, msg="2$")
                break
            else:
                print(strData+"\n")
                respuesta = input("Ingresa la respuesta: ")
                outMsg(client=ClientSocket, msg=respuesta +"$")

                Answer = inMsg(ClientSocket) 
                print("--------------------------------")
                print(Answer)

                outMsg(client=ClientSocket, msg="$")
            
            #Espera 3 segundos para mostrar la respuesta
            time.sleep(3)

def inMsg(client):
    strMsg = ""
    byteData = client.recv(64)
    strData = byteData.decode('UTF-8')

    for i in strData:   
        if(i != "$"):
            strMsg += i
        else:
            break

    return strMsg 

def outMsg(client, msg):
    client.send(msg.encode('utf-8'))

def Main():
    ClientSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    ClientSocket.connect(("127.0.0.1", 8888))


    strData = inMsg(ClientSocket) 
    os.system('cls')
    #Enviar el nombre del jugador
    clientName = input(strData+"\n")
    player = ClassPlayer(clientName)
    outMsg(client=ClientSocket,msg=clientName+"$")

    #Recibe si es el primer jugador o no
    player.WaitingPlayers(ClientSocket=ClientSocket)

    player.Trivia(ClientSocket=ClientSocket)

    #Mienstras los demas jugadores terminan hay que dejar en espera
    while(True):
        strData = inMsg(ClientSocket) 
        if(strData=="True"):
            outMsg(client=ClientSocket, msg="$")
            break
        else:
            os.system('cls')
            print("Esperando a que los otros jugadores finalicen")
            outMsg(client=ClientSocket, msg="continue$")

        time.sleep(1)

        
    os.system('cls')
    print("\n\tJUEGO TERMINADO")
    print("-------------------------------")

    #se Reciben los o el ganador
    Winners = inMsg(ClientSocket) 

    print(Winners)

    input("\nPresione enter para finalizar...")
    outMsg(client=ClientSocket, msg="delete$")

    ClientSocket.close()
Main()