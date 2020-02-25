import socket
import keyboard


if __name__ == "__main__":

    initPlayer = "(init Bobbers)"
    movePlayer = "(move -20 0)"
    bytesToInit = str.encode(initPlayer)
    bytesToMove = str.encode(movePlayer)
    byteDash = str.encode("(dash 100)")
    byteKick = str.encode("(kick 100 0)")
    byteLightKick = str.encode("(kick 25)")
    byteDashRight = str.encode("(dash 100 90)")
    byteDashLeft = str.encode("(dash 100 -90)")
    byteDashBack = str.encode("(dash 100 180)")
    byteRotateRight = str.encode("(turn 20)")
    byteRotateLeft = str.encode("(turn -20)")
    byteCatch = str.encode("(catch 0)")
    byteTackle = str.encode("(tackle 0)")

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

    # Send to server using created UDP socket
    UDPClient1Socket.sendto(str.encode("(move -50 0)"), serverAddressPort)
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

        try:  # used try so that if user pressed other than the given key error will not be shown
            if keyboard.is_pressed('w') or keyboard.is_pressed("up"):
                UDPClient1Socket.sendto(byteDash, serverAddressPort)
                continue
            if keyboard.is_pressed('a') or keyboard.is_pressed("down"):
                UDPClient1Socket.sendto(byteDashLeft, serverAddressPort)
                continue
            if keyboard.is_pressed('d'):
                UDPClient1Socket.sendto(byteDashRight, serverAddressPort)
                continue
            if keyboard.is_pressed('s'):
                UDPClient1Socket.sendto(byteDashBack, serverAddressPort)
                continue
            if keyboard.is_pressed('space'):
                UDPClient1Socket.sendto(byteKick, serverAddressPort)
                continue
            if keyboard.is_pressed('ctrl'):
                UDPClient1Socket.sendto(byteCatch, serverAddressPort)
                continue
            if keyboard.is_pressed('shift'):
                UDPClient1Socket.sendto(byteTackle, serverAddressPort)
                continue
            if keyboard.is_pressed('left'):
                UDPClient1Socket.sendto(byteRotateLeft, serverAddressPort)
                continue
            if keyboard.is_pressed('right'):
                UDPClient1Socket.sendto(byteRotateRight, serverAddressPort)
                continue
        except:
            continue

        msgFromServer = UDPClient1Socket.recvfrom(bufferSize)

        msg = "Message from Server {}".format(msgFromServer[0])

        #print(msg)
