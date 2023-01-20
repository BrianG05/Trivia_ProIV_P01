import socket

class ClassPlayer:
    def __init__(self, PlayerName):
        self.PlayerName = PlayerName
    

def inMsg(client):
    byteData = client.recv(64)
    strData = byteData.decode('UTF-8')

    return strData 

def outMsg(client, msg):
    client.send(msg.encode('utf-8'))

def Main():
    ClientSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    ClientSocket.connect(("127.0.0.1", 8888))

    byteData = ClientSocket.recv(64)
    strData = byteData.decode('UTF-8')  

    #Enviar el nombre del jugador
    clientName = input(strData+"\n")
    player = ClassPlayer(clientName)
    outMsg(client=ClientSocket,msg=clientName+"$")


Main()