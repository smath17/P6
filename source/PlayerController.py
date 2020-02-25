import socket


class Player:

    def __init__(self, teamname):
        self.buffersize = 1024

        # local hosted server on port 6000 by default
        self.serverAddressPort = ("127.0.0.1", 6000)

        # Create client via UDP socket
        self.UDPClientSocket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)

        # Initialize player on team
        self.initString = "(init " + teamname + ")"
        self.send_action(self.initString)

    # Wrapper function for UDP communication + byte encoding
    def send_action(self, action):
        self.UDPClientSocket.sendto(str.encode(action), self.serverAddressPort)

    def rec_msg(self):
        self.UDPClientSocket.recvfrom(self.buffersize)
