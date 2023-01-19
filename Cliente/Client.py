import socket

class ClassPlayer:
    def __init__(self, PlayerName):
        self.PlayerName = PlayerName



def Main():
    host = "127.0.0.1"
    port = 8888

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as client:
        client.connect((host, port))

        byteData = client.recv(64)
        strData = byteData.decode('UTF-8')  


        #Esperar el nombre del jugador
        clientName = input(strData+"\n")
        player = ClassPlayer(clientName)
        client.sendall(clientName.encode('utf-8'))


        byteData = client.recv(64)
        countPlayers = byteData.decode('UTF-8')  

        if(countPlayers == 1):

            i = int(input("Ingrese 1 si desea iniciar el juego: "))
            if(i == 1):
                pass
            
            
        else:
            print("No eres el primer jugador")

Main()