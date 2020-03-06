import socket


class Coach:
    def __init__(self, teamname, offline=False):

        if offline:
            # Server needs to run with -server::coach=on, to disable the referee
            self.coach_port = 6001
            print("Requires -server::coach=on")
        else:
            self.coach_port = 6002

        # TODO. IP of the day
        self.serverAddressPort = ("172.31.253.241", self.coach_port)

        # Create client via UDP socket
        self.UDPClientSocket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)

        self.init_string = "(init " + teamname + " (version 16))"
        self.send_action(self.init_string)

    def send_action(self, action):
        # action is null terminated because server is written in c++
        self.UDPClientSocket.sendto(str.encode(action + '\0'), self.serverAddressPort)
