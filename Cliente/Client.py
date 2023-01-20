import socket

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

    #Enviar el nombre del jugador
    clientName = input(strData+"\n")
    player = ClassPlayer(clientName)
    outMsg(client=ClientSocket,msg=clientName+"$")

    #Recibe si es el primer jugador o no

Main()