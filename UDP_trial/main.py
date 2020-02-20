import socket

if __name__ == "__main__":

    initPlayer = "(init Bobbers)"
    movePlayer = "(move 0, 0)"
    bytesToInit = str.encode(initPlayer)
    bytesToMove = str.encode(movePlayer)

    serverAddressPort = ("127.0.0.1", 6000)

    bufferSize = 1024

    # Create a UDP socket at client side
    UDPClientSocket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)

    # Send to server using created UDP socket
    UDPClientSocket.sendto(bytesToInit, serverAddressPort)
    UDPClientSocket.sendto(bytesToMove, serverAddressPort)

    msgFromServer = UDPClientSocket.recvfrom(bufferSize)

    msg = "Message from Server {}".format(msgFromServer[0])

    print(msg)
    print("You looked")