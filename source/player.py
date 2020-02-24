import socket

class player:

    def __init__(self, teamname):

        # local hosted server on port 6000 by default
        self.serverAddressPort = ("127.0.0.1", 6000)
        # Server messages has to be sent as bytes
        self.initBytes = str.encode("(init" + teamname + ")")


        # Create client via UDP socket
        self.UDPClientSocket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)

        # Initialize player on team
        self.UDPClientSocket.sendto(self.initBytes, self.serverAddressPort)

    # Wrapper function for UDP communication + byte encoding
    def send_action(self, action):
        self.UDPClientSocket.sendto(str.encode(action), self.serverAddressPort)