import socket
from ServerParser import Parser


class Player:

    # goalie of optional, default is false
    def __init__(self, teamname, goalie=False):
        # Player attributes
        self.stamina = 0
        self.speed = 0
        self.effort = 0
        self.head_angle = 0
        self.tackled = 0
        self.game_status = ""
        self.observables = []
        self.ip_address = "172.31.253.196"

        # Instantiate parser to update info
        self.parser = Parser()

        # Server on port 6000 by default
        # TODO. IP of the day
        self.init_port = 6000
        self.serverAddressPort = (self.ip_address, self.init_port)

        # Create client via UDP socket
        self.UDPClientSocket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)

        # Initialize player on team
        if not goalie:
            self.initString = "(init " + teamname + " (version 16)" + ")"
        else:
            self.initString = "(init " + teamname + " (version 16) (goalie))"

        self.send_init(self.initString)

    # Wrapper function for UDP communication + byte encoding
    def send_init(self, action):
        self.send_action(action)
        # Receive server init output and grab dedicated port
        new_port = self.UDPClientSocket.recvfrom(6000)[1]
        self.serverAddressPort = (self.ip_address, new_port[1])
        print(new_port)

    def send_action(self, action):
        # action is null terminated because server is written in c++
        self.UDPClientSocket.sendto(str.encode(action + '\0'), self.serverAddressPort)
        print(self.rec_msg())

    def rec_msg(self):
        # Receive message from server, decode from bytes
        msg_from_server = self.UDPClientSocket.recvfrom(6000)

        # the received msg is (bytes, encoding) we just want the bytes, hence [0]
        return msg_from_server[0].decode()

    def update_info(self):
        self.observables = self.parser.parse_info(self.rec_msg(), self)

    def disconnect(self):
        self.send_action("(bye)")
