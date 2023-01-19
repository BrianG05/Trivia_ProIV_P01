import socket

host = "127.0.0.1"
port = 8888

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as client:
    client.connect((host, port))

    byteData = client.recv(64)
    strData = byteData.decode('UTF-8')  

    clientName = input(strData+"\n")

    client.sendall(clientName.encode('utf-8'))
