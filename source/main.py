import socket

if __name__ == "__main__":

    initPlayer = "(init Bobbers)"
    movePlayer = "(move -20 0)"
    bytesToInit = str.encode(initPlayer)
    bytesToMove = str.encode(movePlayer)
    bytesDash = str.encode("(dash 100)")
    byteKick = str.encode("(kick 20 0)")

    serverAddressPort = ("127.0.0.1", 6000)

    bufferSize = 1024

    # Create a UDP socket at client side
    UDPClient1Socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
    UDPClient2Socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
    UDPClient3Socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
    UDPClient4Socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
    UDPClient5Socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
    UDPClient6Socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
    UDPClient7Socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
    UDPClient8Socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
    UDPClient9Socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
    UDPClient10Socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
    UDPClient11Socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)

    # Send to server using created UDP socket
    UDPClient1Socket.sendto(bytesToInit, serverAddressPort)
    UDPClient2Socket.sendto(bytesToInit, serverAddressPort)
    UDPClient3Socket.sendto(bytesToInit, serverAddressPort)
    UDPClient4Socket.sendto(bytesToInit, serverAddressPort)
    UDPClient5Socket.sendto(bytesToInit, serverAddressPort)
    UDPClient6Socket.sendto(bytesToInit, serverAddressPort)
    UDPClient7Socket.sendto(bytesToInit, serverAddressPort)
    UDPClient8Socket.sendto(bytesToInit, serverAddressPort)
    UDPClient9Socket.sendto(bytesToInit, serverAddressPort)
    UDPClient10Socket.sendto(bytesToInit, serverAddressPort)
    UDPClient11Socket.sendto(bytesToInit, serverAddressPort)

    UDPClient1Socket.sendto(bytesToMove, serverAddressPort)
    UDPClient2Socket.sendto(bytesToMove, serverAddressPort)
    UDPClient3Socket.sendto(bytesToMove, serverAddressPort)
    UDPClient4Socket.sendto(bytesToMove, serverAddressPort)
    UDPClient5Socket.sendto(bytesToMove, serverAddressPort)
    UDPClient6Socket.sendto(bytesToMove, serverAddressPort)
    UDPClient7Socket.sendto(bytesToMove, serverAddressPort)
    UDPClient8Socket.sendto(bytesToMove, serverAddressPort)
    UDPClient9Socket.sendto(bytesToMove, serverAddressPort)
    UDPClient10Socket.sendto(bytesToMove, serverAddressPort)
    UDPClient11Socket.sendto(bytesToMove, serverAddressPort)

    while True:
        #UDPClient4Socket.sendto(bytesToMove, serverAddressPort)
        UDPClient4Socket.sendto(byteDash, serverAddressPort)

        msgFromServer = UDPClient4Socket.recvfrom(bufferSize)

        msg = "Message from Server {}".format(msgFromServer[0])

        print(msg)
