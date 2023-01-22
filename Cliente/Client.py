import os
import socket
import time

class ClassPlayer:
    def __init__(self, PlayerName):
        self.PlayerName = PlayerName
    

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

    #Bloque correspondiente a la espera de los demas clientes
    while(True):
        strData = inMsg(ClientSocket)

        if(strData == "1"):
            input("Presione enter para inciar juego")
            outMsg(client=ClientSocket,msg="1$")
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

    print("Ya puede cerrar la ostia paire!")
Main()